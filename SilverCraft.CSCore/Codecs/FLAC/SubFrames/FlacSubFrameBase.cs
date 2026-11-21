using System.Diagnostics;
using SilverCraft.CSCore.Utils;

// ReSharper disable once CheckNamespace
namespace SilverCraft.CSCore.Codecs.FLAC.SubFrames;

internal class FlacSubFrameBase
{
    public static bool Decode(ref SpanBitReader reader, FlacSubFrameData data, FlacFrameHeader header, int bitsPerSample)
    {
        var wastedBits = 0;
        var firstByte = reader.ReadBits(8);

        if ((firstByte & 0x80) != 0) // Zero bit padding, to prevent sync-fooling string of 1s
        {
            Debug.WriteLine("Flacdecoder subframe-header got no zero-bit padding.");
            return false;
        }

        var hasWastedBits = (firstByte & 1) != 0; // Wasted bits-per-sample flag
        if (hasWastedBits)
        {
            var k = (int)reader.ReadUnary();
            wastedBits = k + 1; // "k-1" follows -> add 1
            bitsPerSample -= wastedBits;
        }

        var subframeType = (firstByte & 0x7E) >> 1; // 0111 1110

        switch (subframeType)
        {
            case 0:
                FlacSubFrameConstant.Decode(ref reader, header, data, bitsPerSample);
                break;
            case 1:
                FlacSubFrameVerbatim.Decode(ref reader, header, data, bitsPerSample);
                break;
            default:
            {
                if ((subframeType & 0x20) != 0) // 100000 = 0x20
                {
                    var order = (int)(subframeType & 0x1F) + 1;
                    FlacSubFrameLPC.Decode(ref reader, header, data, bitsPerSample, order);
                }
                else if ((subframeType & 0x08) != 0) // 001000 = 0x08
                {
                    var order = (int)(subframeType & 0x07);
                    if (order > 4) return false;
                    FlacSubFrameFixed.Decode(ref reader, header, data, bitsPerSample, order);
                }
                else
                {
                    Debug.WriteLine($"Invalid Flac-SubframeType. SubframeType: 0x{subframeType:x}.");
                    return false;
                }

                break;
            }
        }

        if (hasWastedBits)
        {
            var destination = data.DestinationSpan;
            var blockSize = header.BlockSize;
            for (var i = 0; i < blockSize; i++)
            {
                destination[i] <<= wastedBits;
            }
        }

        return true;
    }

#if FLAC_DEBUG
    public static FlacSubFrameBase? GetSubFrame(ref SpanBitReader reader, FlacSubFrameData data, FlacFrameHeader header, int bitsPerSample)
    {
        int wastedBits = 0, order;

        var firstByte = reader.ReadBits(8);

        if ((firstByte & 0x80) != 0) // Zero bit padding, to prevent sync-fooling string of 1s
        {
            Debug.WriteLine("Flacdecoder subframe-header got no zero-bit padding.");
            return null;
        }

        var hasWastedBits = (firstByte & 1) != 0; // Wasted bits-per-sample flag
        if (hasWastedBits)
        {
            var k = (int)reader.ReadUnary();
            wastedBits = k + 1; // "k-1" follows -> add 1
            bitsPerSample -= wastedBits;
        }

        FlacSubFrameBase subFrame;
        var subframeType = (firstByte & 0x7E) >> 1; // 0111 1110

        switch (subframeType)
        {
            case 0:
                subFrame = new FlacSubFrameConstant(ref reader, header, data, bitsPerSample);
                break;
            case 1:
                subFrame = new FlacSubFrameVerbatim(ref reader, header, data, bitsPerSample);
                break;
            default:
            {
                if ((subframeType & 0x20) != 0) // 100000 = 0x20
                {
                    order = (int)(subframeType & 0x1F) + 1;
                    subFrame = new FlacSubFrameLPC(ref reader, header, data, bitsPerSample, order);
                }
                else if ((subframeType & 0x08) != 0) // 001000 = 0x08
                {
                    order = (int)(subframeType & 0x07);
                    if (order > 4) return null;
                    subFrame = new FlacSubFrameFixed(ref reader, header, data, bitsPerSample, order);
                }
                else
                {
                    Debug.WriteLine($"Invalid Flac-SubframeType. SubframeType: 0x{subframeType:x}.");
                    return null;
                }

                break;
            }
        }

        if (hasWastedBits)
        {
            var destination = data.DestinationSpan;
            var blockSize = header.BlockSize;
            for (var i = 0; i < blockSize; i++)
            {
                destination[i] <<= wastedBits;
            }
        }

        subFrame.WastedBits = wastedBits;
        return subFrame;
    }

    public int WastedBits { get; private set; }

    public FlacFrameHeader Header { get; private set; }

    protected FlacSubFrameBase(FlacFrameHeader header)
    {
        Header = header;
    }
#endif
}