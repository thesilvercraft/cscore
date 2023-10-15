using System;
using System.IO;

namespace CSCore.Utils
{
    internal class ReadBlockStream : Stream
    {
        private long _position;

        private readonly Stream _stream;

        public ReadBlockStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (stream.CanRead == false)
                throw new ArgumentException("Can't read stream");

            _stream = stream;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = 0;
            while (read < count)
            {
                read += _stream.Read(buffer, offset + read, count - read);
            }

            _position += read;
            return count;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override void Flush()
        {
            throw new InvalidOperationException();
        }

        public override long Length => _stream.Length;

        public override long Position
        {
            get => _position;
            set => throw new InvalidOperationException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }
    }
}