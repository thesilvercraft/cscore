using System;
using System.IO;

namespace SilverCraft.CSCore.Tags.ID3
{
    public class ID3v2Header
    {
        public const int HeaderLength = 10;

        public static ID3v2Header FromStream(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!stream.CanRead) throw new ArgumentException("stream not readable");
            if (!stream.CanSeek) throw new ArgumentException("stream not seekable");

            var start = stream.Position;
            var buffer = new byte[HeaderLength];
            var read = 0;
            while (read < 3)
            {
                var count = stream.Read(buffer, read, 3 - read);
                if (count == 0)
                {
                    stream.Position = start;
                    return null;
                }
                read += count;
            }

            if (buffer[0] != 0x49 || buffer[1] != 0x44 || buffer[2] != 0x33)
            {
                stream.Position = start;
                return null;
            }

            while (read < HeaderLength)
            {
                var count = stream.Read(buffer, read, HeaderLength - read);
                if (count == 0)
                    throw new EndOfStreamException();
                read += count;
            }

            return new ID3v2Header
            {
                Version = (ID3Version)buffer[3],
                RawVersion = [buffer[3], buffer[4]],
                Flags = (ID3v2HeaderFlags)buffer[5],
                DataLength = ID3Utils.ReadInt32(buffer, 6, true)
            };
        }
        private ID3v2Header()
        {
        }

        public ID3Version Version { get; private set; }

        public byte[] RawVersion { get; private set; }

        public int DataLength { get; private set; }

        public ID3v2HeaderFlags Flags { get; private set; }

        public bool IsUnsync => (Flags & ID3v2HeaderFlags.Unsynchronisation) == ID3v2HeaderFlags.Unsynchronisation;
    }
}
