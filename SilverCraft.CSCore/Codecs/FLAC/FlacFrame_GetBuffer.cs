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
    var desiredSize = Header.BlockSize * Header.Channels * ((Header.BitsPerSample + 7) / 2);
    if (buffer == null || buffer.Length < desiredSize)
    {
        if (buffer != null) ArrayPool<byte>.Shared.Return(buffer);
        buffer = ArrayPool<byte>.Shared.Rent(desiredSize);
    }

    Span<byte> outSpan = buffer;
    var channels = Header.Channels;
    var blockSize = Header.BlockSize;

    var byteIndex = 0;

    switch (Header.BitsPerSample)
    {
        case 8:
            for (var i = 0; i < blockSize; i++)
            for (var c = 0; c < channels; c++)
            {
                outSpan[byteIndex++] = (byte)(_subFrameData[c].DestinationSpan[i] + 0x80);
            }
            break;

        case 16:
            for (var i = 0; i < blockSize; i++)
            for (var c = 0; c < channels; c++)
            {
                var vals = (short)_subFrameData[c].DestinationSpan[i];

                outSpan[byteIndex++] = (byte)(vals & 0xFF);
                outSpan[byteIndex++] = (byte)((vals >> 8) & 0xFF);
            }
            break;

        case 24:
            for (var i = 0; i < blockSize; i++)
            for (var c = 0; c < channels; c++)
            {
                var vali = _subFrameData[c].DestinationSpan[i];

                outSpan[byteIndex++] = (byte)(vali & 0xFF);
                outSpan[byteIndex++] = (byte)((vali >> 8) & 0xFF);
                outSpan[byteIndex++] = (byte)((vali >> 16) & 0xFF);
            }
            break;

        default:
            throw new FlacException(
                $"FlacFrame::GetBuffer: Invalid BitsPerSample value: {Header.BitsPerSample}", FlacLayer.Frame);
    }

    return byteIndex;
}
}