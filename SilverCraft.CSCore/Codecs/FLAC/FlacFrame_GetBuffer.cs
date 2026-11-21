using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace SilverCraft.CSCore.Codecs.FLAC;

public partial class FlacFrame
{
    internal int RequiredPcmBytes =>
        Header.BlockSize * Header.Channels * ((Header.BitsPerSample + 7) / 8);

    /// <summary>
    ///     Gets the raw pcm data of the flac frame.
    /// </summary>
    /// <param name="buffer">The buffer which should be used to store the data in. This value can be null.</param>
    /// <returns>The number of read bytes.</returns>
    public int GetBuffer(ref byte[]? buffer)
    {
        var desiredSize = RequiredPcmBytes;

        if (buffer == null || buffer.Length < desiredSize)
        {
            if (buffer != null) ArrayPool<byte>.Shared.Return(buffer);
            buffer = ArrayPool<byte>.Shared.Rent(desiredSize);
        }

        if (desiredSize == 0)
            return 0;

        return WriteBuffer(buffer.AsSpan(0, desiredSize));
    }

    internal int WriteBuffer(Span<byte> output)
    {
        var channels = Header.Channels;
        var blockSize = Header.BlockSize;
        var bytesPerSample = (Header.BitsPerSample + 7) / 8;
        var desiredSize = blockSize * channels * bytesPerSample;

        if (desiredSize == 0)
            return 0;

        ArgumentOutOfRangeException.ThrowIfLessThan(output.Length, desiredSize);

        ReadOnlySpan<int> source = _destBuffer.AsSpan(0, channels * blockSize);
        ref var srcRef = ref MemoryMarshal.GetReference(source);
        ref var dstRef = ref MemoryMarshal.GetReference(output);

        switch (Header.BitsPerSample)
        {
            case 8:
            {
                if (channels == 2)
                {
                    ref var rightRef = ref Unsafe.Add(ref srcRef, blockSize);
                    for (var i = 0; i < blockSize; i++)
                    {
                        var outOffset = i * 2;
                        Unsafe.Add(ref dstRef, outOffset) = (byte)(Unsafe.Add(ref srcRef, i) + 0x80);
                        Unsafe.Add(ref dstRef, outOffset + 1) = (byte)(Unsafe.Add(ref rightRef, i) + 0x80);
                    }
                }
                else
                {
                    var byteIndex = 0;
                    for (var i = 0; i < blockSize; i++)
                    {
                        for (var c = 0; c < channels; c++)
                        {
                            var value = Unsafe.Add(ref srcRef, (c * blockSize) + i);
                            Unsafe.Add(ref dstRef, byteIndex++) = (byte)(value + 0x80);
                        }
                    }
                }

                return desiredSize;
            }

            case 16:
            {
                if (channels == 2 && BitConverter.IsLittleEndian)
                {
                    ref var rightRef = ref Unsafe.Add(ref srcRef, blockSize);
                    var i = 0;

                    if (Vector256.IsHardwareAccelerated && blockSize >= Vector256<int>.Count)
                    {
                        var mask = Vector256.Create(0xFFFFu);
                        var limit = blockSize - Vector256<int>.Count;
                        for (; i <= limit; i += Vector256<int>.Count)
                        {
                            var left = Vector256.LoadUnsafe(ref srcRef, (nuint)i).AsUInt32();
                            var right = Vector256.LoadUnsafe(ref rightRef, (nuint)i).AsUInt32();
                            var packed = (left & mask) | (right << 16);
                            packed.AsByte().StoreUnsafe(ref dstRef, (nuint)(i * 4));
                        }
                    }

                    if (Vector128.IsHardwareAccelerated && i <= blockSize - Vector128<int>.Count)
                    {
                        var mask128 = Vector128.Create(0xFFFFu);
                        var limit128 = blockSize - Vector128<int>.Count;
                        for (; i <= limit128; i += Vector128<int>.Count)
                        {
                            var left = Vector128.LoadUnsafe(ref srcRef, (nuint)i).AsUInt32();
                            var right = Vector128.LoadUnsafe(ref rightRef, (nuint)i).AsUInt32();
                            var packed = (left & mask128) | (right << 16);
                            packed.AsByte().StoreUnsafe(ref dstRef, (nuint)(i * 4));
                        }
                    }

                    for (; i < blockSize; i++)
                    {
                        var l = (ushort)(short)Unsafe.Add(ref srcRef, i);
                        var r = (ushort)(short)Unsafe.Add(ref rightRef, i);
                        var packed = (uint)l | ((uint)r << 16);
                        Unsafe.WriteUnaligned(ref Unsafe.Add(ref dstRef, i * 4), packed);
                    }
                }
                else
                {
                    var byteIndex = 0;
                    for (var i = 0; i < blockSize; i++)
                    {
                        for (var c = 0; c < channels; c++)
                        {
                            var value = (short)Unsafe.Add(ref srcRef, (c * blockSize) + i);
                            Unsafe.Add(ref dstRef, byteIndex++) = (byte)value;
                            Unsafe.Add(ref dstRef, byteIndex++) = (byte)(value >> 8);
                        }
                    }
                }

                return desiredSize;
            }

            case 24:
            {
                if (channels == 2)
                {
                    ref var rightRef = ref Unsafe.Add(ref srcRef, blockSize);
                    var byteIndex = 0;
                    for (var i = 0; i < blockSize; i++)
                    {
                        var lv = Unsafe.Add(ref srcRef, i);
                        Unsafe.Add(ref dstRef, byteIndex) = (byte)lv;
                        Unsafe.Add(ref dstRef, byteIndex + 1) = (byte)(lv >> 8);
                        Unsafe.Add(ref dstRef, byteIndex + 2) = (byte)(lv >> 16);

                        var rv = Unsafe.Add(ref rightRef, i);
                        Unsafe.Add(ref dstRef, byteIndex + 3) = (byte)rv;
                        Unsafe.Add(ref dstRef, byteIndex + 4) = (byte)(rv >> 8);
                        Unsafe.Add(ref dstRef, byteIndex + 5) = (byte)(rv >> 16);

                        byteIndex += 6;
                    }
                }
                else
                {
                    var byteIndex = 0;
                    for (var i = 0; i < blockSize; i++)
                    {
                        for (var c = 0; c < channels; c++)
                        {
                            var value = Unsafe.Add(ref srcRef, (c * blockSize) + i);
                            Unsafe.Add(ref dstRef, byteIndex++) = (byte)value;
                            Unsafe.Add(ref dstRef, byteIndex++) = (byte)(value >> 8);
                            Unsafe.Add(ref dstRef, byteIndex++) = (byte)(value >> 16);
                        }
                    }
                }

                return desiredSize;
            }

            default:
                throw new FlacException(
                    $"FlacFrame::GetBuffer: Invalid BitsPerSample value: {Header.BitsPerSample}",
                    FlacLayer.Frame);
        }
    }
}