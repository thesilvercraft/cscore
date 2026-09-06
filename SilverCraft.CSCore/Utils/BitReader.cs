
namespace SilverCraft.CSCore.Utils
{
    /// <summary>
    /// This class is based on the CUETools.NET BitReader (see http://sourceforge.net/p/cuetoolsnet/code/ci/default/tree/CUETools.Codecs/BitReader.cs, now located at https://github.com/gchudov/cuetools.net/blob/master/CUETools.Codecs/BitReader.cs)
    /// The author "Grigory Chudov" explicitly gave the permission to use the source as part of the cscore source code which got licensed under the ms-pl.
    /// </summary>
    internal  class BitReader 
    {
        private int _bitoffset;
        private int _bufferOffset;
        private byte[] _buffer;
        private uint _cache;
        private int _position;

        public BitReader(byte[] buffer, int offset)
        {
            if (buffer is not { Length: > 0 })
                throw new ArgumentException("buffer is null or has no elements", nameof(buffer));
            ArgumentOutOfRangeException.ThrowIfNegative(offset);
            _buffer = buffer;
            _cache = PeekCache();
        }

        protected internal uint Cache => _cache;

        public byte[] Buffer => _buffer;

        public int Position => _position;

       
        private uint PeekCache()
        {
            unchecked
            {
                uint b0 = _bufferOffset < _buffer.Length ? _buffer[_bufferOffset] : (uint)0;
                uint b1 = _bufferOffset + 1 < _buffer.Length ? _buffer[_bufferOffset + 1] : (uint)0;
                uint b2 = _bufferOffset + 2 < _buffer.Length ? _buffer[_bufferOffset + 2] : (uint)0;
                uint b3 = _bufferOffset + 3 < _buffer.Length ? _buffer[_bufferOffset + 3] : (uint)0;

                uint result = (b0 << 24) | (b1 << 16) | (b2 << 8) | b3;
                return result << _bitoffset;
            }
        }

        public void SeekBytes(int bytes)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bytes);
            SeekBits(bytes * 8);
        }

        public void SeekBits(int bits)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bits);

            var tmp = _bitoffset + bits;
            _bufferOffset += tmp >> 3;  //skip bytes
            _bitoffset = tmp & 7;     ; //bitoverflow -> max 7 bit

            _position += tmp >> 3;
            _cache = PeekCache();
        }

        public uint ReadBits(int bits)
        {
            if (bits <= 0 || bits > 32)
                throw new ArgumentOutOfRangeException(nameof(bits), "bits has to be a value between 1 and 32");

            var result = _cache >> 32 - bits;
            if (bits <= 24)
            {
                SeekBits(bits);
                return result;
            }

            SeekBits(24);
            result |= _cache >> 56 - bits;
            SeekBits(bits - 24);

            return result;
        }

        public int ReadBitsSigned(int bits)
        {
            if (bits is <= 0 or > 32)
                throw new ArgumentOutOfRangeException(nameof(bits), "bits has to be a value between 1 and 32");

            var result = (int) ReadBits(bits);
            result <<= (32 - bits);
            result >>= (32 - bits);
            return result;
        }

        public ulong ReadBits64(int bits)
        {
            if (bits is <= 0 or > 64)
                throw new ArgumentOutOfRangeException(nameof(bits), "bits has to be a value between 1 and 64");

            ulong result = ReadBits(Math.Min(24, bits));
            if (bits <= 24)
                return result;

            bits -= 24;
            result = (result << bits) | ReadBits(Math.Min(24, bits));
            if (bits <= 24)
                return result;

            bits -= 24;
            return (result << bits) | ReadBits(bits);
        }

        public long ReadBits64Signed(int bits)
        {
            if (bits is <= 0 or > 64)
                throw new ArgumentOutOfRangeException(nameof(bits), "bits has to be a value between 1 and 64");

            var result = (long) ReadBits64(bits);
            result <<= (64 - bits);
            result >>= (64 - bits);
            return result;
        }

        public short ReadInt16()
        {
            return (short) ReadBitsSigned(16);
        }

        public ushort ReadUInt16()
        {
            return (ushort) ReadBits(16);
        }

        public int ReadInt32()
        {
            return ReadBitsSigned(32);
        }


        public uint ReadUInt32()
        {
            return ReadBits(32);
        }

        public ulong ReadUInt64()
        {
            return ReadBits64(64);
        }

        public long ReadInt64()
        {
            return ReadBits64Signed(64);
        }

        public bool ReadBit()
        {
            return ReadBitI() == 1;
        }

        public int ReadBitI()
        {
            var result = _cache >> 31;
            SeekBits(1);
            return (int) result;
        }

        public void Flush()
        {
            if (_bitoffset > 0 && _bitoffset <= 8)
                SeekBits(8 - _bitoffset);
        }

    }
}