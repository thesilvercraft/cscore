using System.Numerics;
using System.Runtime.CompilerServices;

namespace SilverCraft.CSCore.Utils;

/// <summary>
/// This class is based on the CUETools.NET project (see http://sourceforge.net/p/cuetoolsnet/ , now located at https://github.com/gchudov/cuetools.net)
/// The author "Grigory Chudov" explicitly gave the permission to use the source as part of the cscore source code which got licensed under the ms-pl.
/// </summary>
internal abstract class CRCBase<T> where T : struct, IUnsignedNumber<T>, IBinaryInteger<T>
{
    protected readonly T[] crc_table = new T[256];
    protected readonly int bits;

    protected CRCBase(T polynomial)
    {
        bits = Unsafe.SizeOf<T>() * 8;
        if (bits != 8 && bits != 16 && bits != 32)
            throw new NotSupportedException($"CRC for {bits}-bit types is not supported.");

        CalcTable(polynomial);
    }

    private void CalcTable(T polynomial)
    {
        var poly = int.CreateChecked(polynomial) | (1 << bits);
        var bitmask = (1 << bits) - 1;

        for (var i = 0; i < crc_table.Length; i++)
        {
            var crc = i;
            for (var n = 0; n < bits; n++)
            {
                if ((crc & (1 << (bits - 1))) != 0)
                    crc = (crc << 1) ^ poly;
                else
                    crc <<= 1;
            }

            crc_table[i] = T.CreateTruncating(crc & bitmask);
        }
    }

    public abstract T CalcCheckSum(ReadOnlySpan<byte> buffer);

    public abstract T CalcCheckSum(byte[] buffer, int offset, int count);
}