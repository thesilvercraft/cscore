using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace SilverCraft.CSCore.Codecs.AIFF
{
    internal class AiffBinaryReader
    {
        private readonly BinaryReader _binaryReader;

        public AiffBinaryReader(BinaryReader binaryReader)
        {
            ArgumentNullException.ThrowIfNull(binaryReader);
            _binaryReader = binaryReader;
        }

        public double ReadIeeeExtended() => ConvertFromIeeeExtended(ReadBytes(10));

        public int ReadInt32()
        {
            var buffer = ReadBytes(4);
            return (buffer[0] << 24) | (buffer[1] << 16) | (buffer[2] << 8) | buffer[3];
        }

        public uint ReadUInt32()
        {
            var buffer = ReadBytes(4);
            return (uint)((buffer[0] << 24) |
                           (buffer[1] << 16) |
                           (buffer[2] << 8) |
                           buffer[3]);
        }

        public short ReadInt16()
        {
            var buffer = ReadBytes(2);
            return (short)((buffer[0] << 8) | (buffer[1]));
        }

        public ushort ReadUInt16()
        {
            var buffer = ReadBytes(2);
            return (ushort)((buffer[0] << 8) | (buffer[1]));
        }

        public void Skip(long count)
        {
            if (_binaryReader.BaseStream.CanSeek)
                _binaryReader.BaseStream.Seek(count, SeekOrigin.Current);
            else
                _binaryReader.ReadBytes((int)count);
        }

        private byte[] ReadBytes(int count)
        {
            var bytes = _binaryReader.ReadBytes(count);
            if (bytes.Length != count)
            {
                throw new EndOfStreamException(string.Format("Could not read {0} bytes. Only {1} bytes were read.",
                    count, bytes.Length));
            }
            return bytes;
        }

        //copied from aiff.cc -> http://worldfoundry.org/doxygen/html/aiff_8cc_source.html#l00633
        private double ConvertFromIeeeExtended(byte[] bytes)
        {
            double f;
            int expon;
            uint hiMant, loMant;

            expon = ((bytes[0] & 0x7F) << 8) | bytes[1];
            hiMant = (uint)((bytes[2]  << 24)
                     | (bytes[3]  << 16)
                     | (bytes[4]  << 8)
                     | bytes[5]);
            loMant = (uint)((bytes[6]  << 24)
                     | (bytes[7]  << 16)
                     | (bytes[8]  << 8)
                     | bytes[9]);

            if (expon == 0 && hiMant == 0 && loMant == 0)
            {
                f = 0;
            }
            else
            {
                if (expon == 0x7FFF)
                {
                    /* Infinity or NaN */
                    f = double.NaN;
                }
                else
                {
                    expon -= 16383;
                    f = ldexp(UnsignedToFloat(hiMant), expon -= 31);
                    f += ldexp(UnsignedToFloat(loMant), expon -= 32);
                }
            }

            if ((bytes[0] & 0x80) == 0x80)
                return -f;
            return f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double ldexp(double x, int exp) => x * Math.Pow(2, exp);

        //# define UnsignedToFloat(u)         (((double)((long)(u - 2147483647L - 1))) + 2147483648.0)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double UnsignedToFloat(ulong u) => (long)(u - 2147483647L - 1) + 2147483648.0;
    }
}