using System.Buffers;

namespace SilverCraft.CSCore.Codecs.FLAC;

public partial class FlacFrame
{
    /// <summary>
    ///     Gets the raw pcm data of the flac frame.
    /// </summary>
    /// <param name="buffer">The buffer which should be used to store the data in. This value can be null.</param>
    /// <returns>The number of read bytes.</returns>
    public int GetBuffer(ref byte[]? buffer)
    {
        var desiredSize =
            Header.BlockSize *
            Header.Channels *
            ((Header.BitsPerSample + 7) / 8);

        if (buffer == null || buffer.Length < desiredSize)
        {
            if (buffer != null) ArrayPool<byte>.Shared.Return(buffer);
            buffer = ArrayPool<byte>.Shared.Rent(desiredSize);
        }

        Span<byte> output = buffer;
        var source = _destBuffer.AsSpan();

        var channels = Header.Channels;
        var blockSize = Header.BlockSize;
        var byteIndex = 0;

        switch (Header.BitsPerSample)
        {
            case 8:
                for (var i = 0; i < blockSize; i++)
                {
                    for (var c = 0; c < channels; c++)
                    {
                        var value = source[c * blockSize + i];

                        output[byteIndex++] = (byte)(value + 0x80);
                    }
                }

                break;

            case 16:
                for (var i = 0; i < blockSize; i++)
                {
                    for (var c = 0; c < channels; c++)
                    {
                        var value = (short)source[c * blockSize + i];

                        output[byteIndex++] = (byte)value;
                        output[byteIndex++] = (byte)(value >> 8);
                    }
                }

                break;

            case 24:
                for (var i = 0; i < blockSize; i++)
                {
                    for (var c = 0; c < channels; c++)
                    {
                        var value = source[c * blockSize + i];

                        output[byteIndex++] = (byte)value;
                        output[byteIndex++] = (byte)(value >> 8);
                        output[byteIndex++] = (byte)(value >> 16);
                    }
                }

                break;

            default:
                throw new FlacException(
                    $"FlacFrame::GetBuffer: Invalid BitsPerSample value: {Header.BitsPerSample}",
                    FlacLayer.Frame);
        }

        return byteIndex;
    }
}