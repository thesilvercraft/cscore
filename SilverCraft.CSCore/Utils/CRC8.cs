using System.Runtime.CompilerServices;

namespace SilverCraft.CSCore.Utils
{
    /// <summary>
    /// This class is based on the CUETools.NET project (see http://sourceforge.net/p/cuetoolsnet/ , now located at https://github.com/gchudov/cuetools.net)
    /// The author "Grigory Chudov" explicitly gave the permission to use the source as part of the cscore source code which got licensed under the ms-pl.
    /// </summary>
    internal class CRC8() : CRCBase<byte>(0x07)
    {
        public static CRC8 Instance { get; } = new CRC8();

        public override byte CalcCheckSum(ReadOnlySpan<byte> buffer)
        {
            byte res = 0;
            foreach (var b in buffer)
            {
                res = crc_table[res ^ b];
            }
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override byte CalcCheckSum(byte[] buffer, int offset, int count)
        {
            return CalcCheckSum(buffer.AsSpan(offset, count));
        }
    }
}