using System.Buffers;

namespace SilverCraft.CSCore.Codecs.FLAC;

public partial class FlacFrame
{
    /// <summary>
    ///     Gets the raw pcm data of the flac frame.
    /// </summary>
    /// <param name="buffer">The buffer which should be used to store the data in. This value can be null.</param>
    /// <returns>The number of read bytes.</returns>
    public unsafe int GetBuffer(ref byte[]? buffer)
    {
        // Keep your exact original sizing formula so the array allocation match is identical
        var desiredSize = Header.BlockSize * Header.Channels * ((Header.BitsPerSample + 7) / 2);
        if (buffer == null || buffer.Length < desiredSize)
        {
            if (buffer != null) ArrayPool<byte>.Shared.Return(buffer);
            buffer = ArrayPool<byte>.Shared.Rent(desiredSize);
        }

        // Wrap the array in a span for fast, safe indexing
        Span<byte> outSpan = buffer;
        var channels = Header.Channels;
        var blockSize = Header.BlockSize;

        // This tracks the exact byte offset, identical to how *(ptr++) worked
        var byteIndex = 0;

        switch (Header.BitsPerSample)
        {
            case 8:
                for (var i = 0; i < blockSize; i++)
                for (var c = 0; c < channels; c++)
                    outSpan[byteIndex++] = (byte)(_subFrameData[c].DestinationBuffer[i] + 0x80);

                break;

            case 16:
                for (var i = 0; i < blockSize; i++)
                for (var c = 0; c < channels; c++)
                {
                    var vals = (short)_subFrameData[c].DestinationBuffer[i];

                    // Explicitly pack the bytes sequentially to guarantee perfect alignment
                    outSpan[byteIndex++] = (byte)(vals & 0xFF);
                    outSpan[byteIndex++] = (byte)((vals >> 8) & 0xFF);
                }

                break;

            case 24:
                for (var i = 0; i < blockSize; i++)
                for (var c = 0; c < channels; c++)
                {
                    var vali = _subFrameData[c].DestinationBuffer[i];

                    outSpan[byteIndex++] = (byte)(vali & 0xFF);
                    outSpan[byteIndex++] = (byte)((vali >> 8) & 0xFF);
                    outSpan[byteIndex++] = (byte)((vali >> 16) & 0xFF);
                }

                break;

            default:
                throw new FlacException(
                    $"FlacFrame::GetBuffer: Invalid BitsPerSample value: {Header.BitsPerSample}", FlacLayer.Frame);
        }

        // CRITICAL: Return byteIndex (the actual bytes written), NOT desiredSize!
        // This ensures your calling code reads only the clean audio data.
        return byteIndex;
    }
}