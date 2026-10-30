using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace SilverCraft.CSCore.Codecs.WAV
{
    /// <summary>
    ///     Provides a decoder for reading wave files.
    /// </summary>
    public class WaveFileReader : IWaveSource
    {
        private readonly List<WaveFileChunk> _chunks;

        private readonly Lock _lockObj = new();

        private bool _disposed;
        private Stream? _stream;
        private readonly DataChunk _dataChunk;
        private readonly bool _closeStream;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WaveFileReader" /> class.
        /// </summary>
        /// <param name="fileName">Filename which points to a wave file.</param>
        public WaveFileReader(string fileName)
            : this(File.OpenRead(fileName))
        {
            _closeStream = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WaveFileReader" /> class.
        /// </summary>
        /// <param name="stream">Stream which contains wave file data.</param>
        public WaveFileReader(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!stream.CanRead)
                throw new ArgumentException("stream is not readable");

            _stream = stream;

            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            if (reader.ReadUInt32() == 0x46464952) // "RIFF" in little-endian byte order.
            {
                reader.ReadInt32(); //FileLength
                reader.ReadChars(4);
            }

            _chunks = ReadChunks(stream);
            _dataChunk = (DataChunk)(_chunks.FirstOrDefault(x => x is DataChunk) ??
                                     throw new ArgumentException(
                                         "The specified stream does not contain any data chunks.", nameof(stream)));
            Position = 0;
        }

        /// <summary>
        ///     Gets a list of all found chunks.
        /// </summary>
        public ReadOnlyCollection<WaveFileChunk> Chunks => _chunks.AsReadOnly();

        /// <summary>
        ///     Reads a sequence of bytes from the <see cref="WaveFileReader" /> and advances the position within the stream by the
        ///     number of bytes read.
        /// </summary>
        /// <param name="buffer">
        ///     An array of bytes. When this method returns, the <paramref name="buffer" /> contains the specified
        ///     byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> +
        ///     <paramref name="count" /> - 1) replaced by the bytes read from the current source.
        /// </param>
        /// <param name="offset">
        ///     The zero-based byte offset in the <paramref name="buffer" /> at which to begin storing the data
        ///     read from the current stream.
        /// </param>
        /// <param name="count">The maximum number of bytes to read from the current source.</param>
        /// <returns>The total number of bytes read into the buffer.</returns>
        public int Read(byte[] buffer, int offset, int count)
        {
            lock (_lockObj)
            {
                CheckForDisposed();

                count = (int)Math.Min(count, _dataChunk.DataEndPosition - _stream.Position);
                count -= count % WaveFormat.BlockAlign;
                return count <= 0 ? 0 : _stream.Read(buffer, offset, count);
            }
        }

        /// <summary>
        ///     Gets the wave format of the wave file. This property gets specified by the <see cref="FmtChunk" />.
        /// </summary>
        public WaveFormat WaveFormat { get; private set; }

        /// <summary>
        ///     Gets or sets the position of the <see cref="WaveFileReader" /> in bytes.
        /// </summary>
        public long Position
        {
            get => _stream != null ? _stream.Position - _dataChunk.DataStartPosition : 0;
            set
            {
                lock (_lockObj)
                {
                    CheckForDisposed();

                    if (value > Length || value < 0)
                        throw new ArgumentOutOfRangeException(nameof(value),
                            "The position must not be bigger than the length or less than zero.");
                    value -= (value % WaveFormat.BlockAlign);
                    _stream.Position = value + _dataChunk.DataStartPosition;
                }
            }
        }

        /// <summary>
        ///     Gets the length of the <see cref="WaveFileReader" /> in bytes.
        /// </summary>
        public long Length => _dataChunk.ChunkDataSize;

        /// <summary>
        /// Gets a value indicating whether the <see cref="WaveFileReader"/> supports seeking.
        /// </summary>
        public bool CanSeek => true;

        /// <summary>
        ///     Disposes the <see cref="WaveFileReader" /> and the underlying stream.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private List<WaveFileChunk> ReadChunks(Stream stream)
        {
            var chunks = new List<WaveFileChunk>(2);
            while (stream.Length - stream.Position >= 8)
            {
                var chunkStart = stream.Position;
                var tmp = WaveFileChunk.FromStream(stream);
                chunks.Add(tmp);

                if (tmp is FmtChunk fmtChunk)
                {
                    WaveFormat = fmtChunk.WaveFormat;
                }

                // RIFF chunk sizes exclude the pad byte used to maintain word alignment. Some WAV
                // writers omit the final data-chunk pad at EOF, so accept that case while still
                // rejecting a truncated chunk payload.
                var chunkDataEnd = chunkStart + 8 + tmp.ChunkDataSize;
                if (chunkDataEnd > stream.Length)
                    throw new EndOfStreamException("The stream ends before the WAV chunk payload is complete.");

                var nextChunkPosition = chunkDataEnd;
                if ((tmp.ChunkDataSize & 1) != 0 && chunkDataEnd < stream.Length)
                    nextChunkPosition++;

                stream.Seek(nextChunkPosition, SeekOrigin.Begin);
            }

            return stream.Position != stream.Length
                ? throw new EndOfStreamException("The stream ends with an incomplete WAV chunk header.")
                : chunks;
        }

        private void CheckForDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }

        /// <summary>
        ///     Disposes the <see cref="WaveFileReader" /> and the underlying stream.
        /// </summary>
        /// <param name="disposing">
        ///     True to release both managed and unmanaged resources; false to release only unmanaged
        ///     resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                lock (_lockObj)
                {
                    if (_stream != null && _closeStream)
                    {
                        _stream.Dispose();
                        _stream = null;
                    }

                    Debug.WriteLine("WaveFile disposed.");
                }
            }

            _disposed = true;
        }

        /// <summary>
        /// Destructor which calls the <see cref="Dispose(bool)"/> method.
        /// </summary>
        ~WaveFileReader()
        {
            Dispose(false);
        }
    }
}