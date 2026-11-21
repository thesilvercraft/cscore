using System.Buffers;
using System.Diagnostics;

namespace SilverCraft.CSCore.Codecs.FLAC
{
    internal readonly record struct FlacSeekEntry(long StreamOffset, long SampleOffset, int BlockSize);

    internal sealed class FlacPreScan
    {
        private const int BufferSize = 65536;
        private readonly Stream _stream;
        private bool _isRunning;
        private FlacMetadataStreamInfo? _streamInfo;
        private List<FlacFrameInformation>? _frames;

        public event EventHandler<FlacPreScanFinishedEventArgs>? ScanFinished;

        public List<FlacSeekEntry> SeekEntries { get; private set; } = [];

        public List<FlacFrameInformation> Frames => _frames ??= BuildFrameInformationList();

        /// <summary>
        /// Total length in bits
        /// </summary>
        public long TotalLengthBits { get; private set; }

        /// <summary>
        /// Total number of samples
        /// </summary>
        public long TotalSamples { get; private set; }

        public FlacPreScan(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!stream.CanRead) throw new ArgumentException("stream is not readable");

            _stream = stream;
        }

        public void ScanStream(FlacMetadataStreamInfo streamInfo, FlacPreScanMode mode, long firstFrameOffset = -1)
        {
            _streamInfo = streamInfo;
            var saveOffset = _stream.Position;
            StartScan(streamInfo, mode, firstFrameOffset);
            _stream.Position = saveOffset;

            long totalLength = 0, totalSamples = 0;
            var entries = SeekEntries;
            var bitsPerSample = streamInfo.BitsPerSample;
            var channels = streamInfo.Channels;
            foreach (var entry in entries)
            {
                totalLength += (long)entry.BlockSize * bitsPerSample * channels;
                totalSamples += entry.BlockSize;
            }

            TotalLengthBits = totalLength;
            TotalSamples = totalSamples;
            Debug.WriteLineIf(TotalSamples != streamInfo.TotalSamples, "SCANNING SAMPLES LED TO DIFFERENCES BETWEEN HEADER AND FILE, POSSIBLY WRONG SCANNING");
            Debug.WriteLineIf(TotalSamples == streamInfo.TotalSamples,
                "Flac prescan successful. Calculated total_samples value matching the streaminfo-metadata.");
        }

        private void StartScan(FlacMetadataStreamInfo streamInfo, FlacPreScanMode mode, long firstFrameOffset)
        {
            if (_isRunning)
                throw new Exception("Scan is already running.");

            _isRunning = true;

            if (mode == FlacPreScanMode.Async)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    SeekEntries = RunScan(streamInfo, firstFrameOffset);
                    _isRunning = false;
                });
            }
            else
            {
                SeekEntries = RunScan(streamInfo, firstFrameOffset);
                _isRunning = false;
            }
        }

        private List<FlacSeekEntry> RunScan(FlacMetadataStreamInfo streamInfo, long firstFrameOffset)
        {
#if FLAC_DEBUG
            var watch = Stopwatch.StartNew();
#endif
            var result = ScanStreamInternal(streamInfo, firstFrameOffset);

#if FLAC_DEBUG
            watch.Stop();
            Debug.WriteLine("FlacPreScan finished: {0} Bytes processed in {1} ms. {2} frames", _stream.Length,
                watch.ElapsedMilliseconds, result.Count);
#endif
            if (ScanFinished != null)
            {
                _frames = BuildFrameInformationList(result, streamInfo);
                RaiseScanFinished(_frames);
            }

            return result;
        }

        private void RaiseScanFinished(List<FlacFrameInformation> frames)
        {
            ScanFinished?.Invoke(this, new FlacPreScanFinishedEventArgs(frames));
        }

        private List<FlacFrameInformation> BuildFrameInformationList()
            => BuildFrameInformationList(SeekEntries, _streamInfo);

        private List<FlacFrameInformation> BuildFrameInformationList(List<FlacSeekEntry> entries, FlacMetadataStreamInfo? streamInfo)
        {
            var list = new List<FlacFrameInformation>(entries.Count);
            if (entries.Count == 0)
                return list;

            var saveOffset = _stream.Position;
            try
            {
                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    _stream.Position = entry.StreamOffset;
                    var header = new FlacFrameHeader(_stream, streamInfo, doCrc: false);
                    list.Add(new FlacFrameInformation
                    {
                        IsFirstFrame = i == 0,
                        StreamOffset = entry.StreamOffset,
                        SampleOffset = entry.SampleOffset,
                        Header = header
                    });
                }
            }
            finally
            {
                _stream.Position = saveOffset;
            }

            return list;
        }

        private List<FlacSeekEntry> ScanStreamInternal(FlacMetadataStreamInfo streamInfo, long firstFrameOffset)
        {
            var stream = _stream;
            if (firstFrameOffset >= 0)
            {
                stream.Position = firstFrameOffset;
            }
            else
            {
                stream.Position = 4; // Skip 'fLaC' marker
                FlacMetadata.SkipMetadata(stream);
                firstFrameOffset = stream.Position;
            }

            var estimatedFrames = streamInfo.MaxBlockSize > 0 && streamInfo.TotalSamples > 0
                ? (int)Math.Min(1_000_000, ((streamInfo.TotalSamples + streamInfo.MaxBlockSize - 1) / streamInfo.MaxBlockSize) + 4)
                : 1024;
            var entries = new List<FlacSeekEntry>(estimatedFrames);
            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            try
            {
                var hasBase = false;
                var baseSampleRate = 0;
                var baseChannels = 0;
                var baseBitsPerSample = 0;
                var baseStrategy = BlockingStrategy.FixedBlockSize;
                long expectedNextFrameOrSample = 0;
                long currentSampleOffset = 0;

                long bufferStreamOffset = firstFrameOffset;
                var validBytes = 0;
                var eof = false;

                while (!eof || validBytes >= 6)
                {
                    if (!eof && validBytes < buffer.Length)
                    {
                        var readNow = stream.Read(buffer, validBytes, buffer.Length - validBytes);
                        if (readNow == 0)
                            eof = true;
                        else
                            validBytes += readNow;
                    }

                    if (validBytes < 6)
                        break;

                    // Keep at least FrameHeaderSize bytes buffered before crossing a refill boundary unless at EOF.
                    var scanLimit = eof ? validBytes - 5 : validBytes - FlacConstant.FrameHeaderSize;
                    if (scanLimit <= 0)
                    {
                        if (eof) break;
                        continue;
                    }

                    var offset = 0;
                    var span = buffer.AsSpan(0, validBytes);

                    while (offset < scanLimit)
                    {
                        var syncRel = span[offset..scanLimit].IndexOf((byte)0xFF);
                        if (syncRel < 0)
                        {
                            offset = scanLimit;
                            break;
                        }

                        offset += syncRel;
                        if ((span[offset + 1] & 0xFE) == 0xF8) // 11111111 111110x0 (sync + reserved bit 0)
                        {
                            var sliceLen = Math.Min(FlacConstant.FrameHeaderSize, validBytes - offset);
                            if (FlacFrameHeader.TryParseCompact(
                                    span.Slice(offset, sliceLen),
                                    streamInfo,
                                    out var blockSize,
                                    out var sampleRate,
                                    out var channels,
                                    out var bitsPerSample,
                                    out var strategy,
                                    out var sampleOrFrameNumber,
                                    out var bytesConsumed))
                            {
                                if (!hasBase)
                                {
                                    hasBase = true;
                                    baseSampleRate = sampleRate;
                                    baseChannels = channels;
                                    baseBitsPerSample = bitsPerSample;
                                    baseStrategy = strategy;
                                    expectedNextFrameOrSample = strategy == BlockingStrategy.FixedBlockSize
                                        ? sampleOrFrameNumber + 1
                                        : sampleOrFrameNumber + blockSize;

                                    entries.Add(new FlacSeekEntry(bufferStreamOffset + offset, currentSampleOffset, blockSize));
                                    currentSampleOffset += blockSize;
                                    offset += Math.Max(1, bytesConsumed);
                                    continue;
                                }

                                if (sampleRate == baseSampleRate &&
                                    channels == baseChannels &&
                                    bitsPerSample == baseBitsPerSample &&
                                    strategy == baseStrategy &&
                                    sampleOrFrameNumber == expectedNextFrameOrSample)
                                {
                                    expectedNextFrameOrSample = strategy == BlockingStrategy.FixedBlockSize
                                        ? sampleOrFrameNumber + 1
                                        : sampleOrFrameNumber + blockSize;

                                    entries.Add(new FlacSeekEntry(bufferStreamOffset + offset, currentSampleOffset, blockSize));
                                    currentSampleOffset += blockSize;
                                    offset += Math.Max(1, bytesConsumed);
                                    continue;
                                }
                            }
                        }

                        offset++;
                    }

                    if (eof)
                        break;

                    var carry = validBytes - offset;
                    if (carry > 0)
                        Buffer.BlockCopy(buffer, offset, buffer, 0, carry);

                    bufferStreamOffset += offset;
                    validBytes = carry;
                }

                return entries;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}