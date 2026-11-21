using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SilverCraft.CSCore.Utils;

/// <summary>
/// High-performance 64-bit buffered big-endian bit reader over a <see cref="ReadOnlySpan{T}"/>.
/// </summary>
internal ref struct SpanBitReader
{
    private readonly ReadOnlySpan<byte> _buffer;
    private ulong _cache;
    private int _bitCount;
    private int _byteIndex;

    public SpanBitReader(ReadOnlySpan<byte> buffer)
    {
        _buffer = buffer;
        _cache = 0;
        _bitCount = 0;
        _byteIndex = 0;
    }

    public readonly int Position => (int)(((long)_byteIndex * 8 - _bitCount) >> 3);

    public readonly bool IsTruncated => ((long)_byteIndex * 8) - _bitCount > (long)_buffer.Length * 8;

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Refill()
    {
        if (_bitCount <= 32 && _byteIndex + 4 <= _buffer.Length)
        {
            uint next = BinaryPrimitives.ReadUInt32BigEndian(_buffer.Slice(_byteIndex));
            _cache |= (ulong)next << (32 - _bitCount);
            _bitCount += 32;
            _byteIndex += 4;
            return;
        }

        while (_bitCount <= 56 && _byteIndex < _buffer.Length)
        {
            _cache |= (ulong)_buffer[_byteIndex++] << (56 - _bitCount);
            _bitCount += 8;
        }

        // Pad with zero bits if reading past end of span (matches BitReader behavior),
        // advancing _byteIndex in lockstep so bit/byte position remains monotonic.
        if (_byteIndex >= _buffer.Length)
        {
            while (_bitCount <= 56)
            {
                _bitCount += 8;
                _byteIndex++;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureBits(int needed)
    {
        if (_bitCount < needed)
            Refill();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SkipBitsInternal(int bits)
    {
        _cache = bits < 64 ? _cache << bits : 0;
        _bitCount -= bits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadBits(int bits)
    {
        if ((uint)(bits - 1) > 31)
            throw new ArgumentOutOfRangeException(nameof(bits), "bits has to be a value between 1 and 32");

        EnsureBits(bits);
        uint result = (uint)(_cache >> (64 - bits));
        SkipBitsInternal(bits);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadBitsSigned(int bits)
    {
        if ((uint)(bits - 1) > 31)
            throw new ArgumentOutOfRangeException(nameof(bits), "bits has to be a value between 1 and 32");

        int result = (int)ReadBits(bits);
        int shift = 32 - bits;
        return (result << shift) >> shift;
    }

    public ulong ReadBits64(int bits)
    {
        if ((uint)(bits - 1) > 63)
            throw new ArgumentOutOfRangeException(nameof(bits), "bits has to be a value between 1 and 64");

        if (bits <= 32)
            return ReadBits(bits);

        ulong hi = ReadBits(32);
        int rem = bits - 32;
        return (hi << rem) | ReadBits(rem);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Flush()
    {
        int bitOffset = ((_byteIndex * 8) - _bitCount) & 7;
        if (bitOffset != 0)
            SkipBitsInternal(8 - bitOffset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUnary()
    {
        uint zeros = 0;
        while (true)
        {
            if (_bitCount == 0)
                Refill();

            int lz = BitOperations.LeadingZeroCount(_cache);
            if (lz < _bitCount)
            {
                SkipBitsInternal(lz + 1);
                return zeros + (uint)lz;
            }

            zeros += (uint)_bitCount;
            _cache = 0;
            _bitCount = 0;

            if (_byteIndex > _buffer.Length)
                return zeros;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadUnarySigned()
    {
        uint value = ReadUnary();
        return (int)((value >> 1) ^ -(int)(value & 1));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadRiceSample(int riceParameter)
    {
        if (riceParameter == 0)
            return ReadUnarySigned();

        if (_bitCount < 32)
            Refill();

        int lz = BitOperations.LeadingZeroCount(_cache);
        if (riceParameter <= 24 && lz + 1 + riceParameter <= _bitCount)
        {
            uint msbs = (uint)lz;
            ulong shifted = _cache << (lz + 1);
            uint lsbs = (uint)(shifted >> (64 - riceParameter));
            uint uval = (msbs << riceParameter) | lsbs;
            int totalBits = lz + 1 + riceParameter;
            SkipBitsInternal(totalBits);
            return (int)((uval >> 1) ^ -(int)(uval & 1));
        }

        uint slowMsbs = ReadUnary();
        uint slowLsbs = ReadBits(riceParameter);
        uint slowUval = (slowMsbs << riceParameter) | slowLsbs;
        return (int)((slowUval >> 1) ^ -(int)(slowUval & 1));
    }

    public bool ReadUTF8_64(out ulong result)
    {
        var x = ReadBits(8);
        ulong v;
        int i;

        if ((x & 0x80) == 0)
        {
            v = x;
            i = 0;
        }
        else if ((x & 0xC0) != 0 && (x & 0x20) == 0)
        {
            v = x & 0x1F;
            i = 1;
        }
        else if ((x & 0xE0) != 0 && (x & 0x10) == 0)
        {
            v = x & 0x0F;
            i = 2;
        }
        else if ((x & 0xF0) != 0 && (x & 0x08) == 0)
        {
            v = x & 0x07;
            i = 3;
        }
        else if ((x & 0xF8) != 0 && (x & 0x04) == 0)
        {
            v = x & 0x03;
            i = 4;
        }
        else if ((x & 0xFC) != 0 && (x & 0x02) == 0)
        {
            v = x & 0x01;
            i = 5;
        }
        else if ((x & 0xFE) != 0 && (x & 0x01) == 0)
        {
            v = 0;
            i = 6;
        }
        else
        {
            result = ulong.MaxValue;
            return false;
        }

        for (; i != 0; i--)
        {
            x = ReadBits(8);
            if ((x & 0xC0) != 0x80)
            {
                result = ulong.MaxValue;
                return false;
            }

            v = (v << 6) | (x & 0x3F);
        }

        result = v;
        return true;
    }

    public bool ReadUTF8_32(out uint result)
    {
        uint v, x;
        int i;

        x = ReadBits(8);
        if ((x & 0x80) == 0)
        {
            v = x;
            i = 0;
        }
        else if ((x & 0xC0) != 0 && (x & 0x20) == 0)
        {
            v = x & 0x1F;
            i = 1;
        }
        else if ((x & 0xE0) != 0 && (x & 0x10) == 0)
        {
            v = x & 0x0F;
            i = 2;
        }
        else if ((x & 0xF0) != 0 && (x & 0x08) == 0)
        {
            v = x & 0x07;
            i = 3;
        }
        else if ((x & 0xF8) != 0 && (x & 0x04) == 0)
        {
            v = x & 0x03;
            i = 4;
        }
        else if ((x & 0xFC) != 0 && (x & 0x02) == 0)
        {
            v = x & 0x01;
            i = 5;
        }
        else
        {
            result = uint.MaxValue;
            return false;
        }

        for (; i != 0; i--)
        {
            x = ReadBits(8);
            if ((x & 0xC0) != 0x80)
            {
                result = uint.MaxValue;
                return false;
            }

            v = (v << 6) | (x & 0x3F);
        }

        result = v;
        return true;
    }
}
