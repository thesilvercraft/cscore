using System.Diagnostics;

namespace SilverCraft.CSCore.Codecs.FLAC
{
    internal sealed class FlacPreScan
    {
        private const int BufferSize = 50000;
        private readonly Stream _stream;
        private bool _isRunning;

        public event EventHandler<FlacPreScanFinishedEventArgs> ScanFinished;

        public List<FlacFrameInformation> Frames { get; private set; }

        public long TotalLength { get; private set; }

        public long TotalSamples { get; private set; }

        public FlacPreScan(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!stream.CanRead) throw new ArgumentException("stream is not readable");

            _stream = stream;
        }

        public void ScanStream(FlacMetadataStreamInfo streamInfo, FlacPreScanMode mode)
        {
            var saveOffset = _stream.Position;
            StartScan(streamInfo, mode);
            _stream.Position = saveOffset;

            long totalLength = 0, totalsamples = 0;
            foreach (var frame in Frames)
            {
                totalLength += frame.Header.BlockSize * frame.Header.BitsPerSample * frame.Header.Channels;
                totalsamples += frame.Header.BlockSize;
            }

            TotalLength = totalLength;
            TotalSamples = totalsamples;
            Debug.Assert(TotalSamples == streamInfo.TotalSamples);
            Debug.WriteLineIf(TotalSamples == streamInfo.TotalSamples,
                "Flac prescan successful. Calculated total_samples value matching the streaminfo-metadata.");
        }

        private void StartScan(FlacMetadataStreamInfo streamInfo, FlacPreScanMode mode)
        {
            if (_isRunning)
                throw new Exception("Scan is already running.");

            _isRunning = true;

            if (mode == FlacPreScanMode.Async)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    Frames = RunScan(streamInfo);
                    _isRunning = false;
                });
            }
            else
            {
                Frames = RunScan(streamInfo);
                _isRunning = false;
            }
        }

        private List<FlacFrameInformation> RunScan(FlacMetadataStreamInfo streamInfo)
        {
#if FLAC_DEBUG
            var watch = new Stopwatch();
            watch.Start();
#endif
            var result = ScanThisShit(streamInfo);

#if FLAC_DEBUG
            watch.Stop();
            Debug.WriteLine("FlacPreScan finished: {0} Bytes processed in {1} ms. {2} frames", _stream.Length,
                watch.ElapsedMilliseconds, result.Count);
#endif
            RaiseScanFinished(result);
            return result;
        }

        private void RaiseScanFinished(List<FlacFrameInformation> frames)
        {
            ScanFinished?.Invoke(this, new FlacPreScanFinishedEventArgs(frames));
        }

        private List<FlacFrameInformation> ScanThisShit(FlacMetadataStreamInfo streamInfo)
        {
            var stream = _stream;
            var buffer = new byte[BufferSize];
    
            stream.Position = 4; // Skip 'fLaC' marker
            FlacMetadata.SkipMetadata(stream);

            var frames = new List<FlacFrameInformation>();
            FlacFrameHeader? baseHeader = null;
            long currentSampleOffset = 0;

            while (true)
            {
                var read = stream.Read(buffer, 0, buffer.Length);
                if (read <= FlacConstant.FrameHeaderSize)
                    break;

                var offset = 0;
                var maxOffset = read - FlacConstant.FrameHeaderSize;

                while (offset < maxOffset)
                {
                    // Sync code check: 11111111 111110xx
                    if (buffer[offset] == 0xFF && (buffer[offset + 1] & 0xF8) == 0xF8)
                    {
                        if (IsFrame(buffer, offset, streamInfo, out FlacFrameHeader header))
                        {
                            baseHeader ??= header;

                            if (baseHeader.IsFormatEqualTo(header))
                            {
                                var frameInfo = new FlacFrameInformation
                                {
                                    IsFirstFrame = (frames.Count == 0),
                                    StreamOffset = stream.Position - read + offset,
                                    SampleOffset = currentSampleOffset,
                                    Header = header
                                };

                                frames.Add(frameInfo);
                                currentSampleOffset += header.BlockSize;
                            }
                        }
                    }

                    offset++;
                }

                stream.Position -= FlacConstant.FrameHeaderSize;
            }

            return frames;
        }

        private bool IsFrame(byte[] buffer, int offset, FlacMetadataStreamInfo streamInfo, out FlacFrameHeader header)
        {
            header = new FlacFrameHeader(buffer, offset, streamInfo, true);
            return !header.HasError;
        }
    }
}