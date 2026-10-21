using System.Runtime.CompilerServices;

namespace SilverCraft.CSCore.Utils
{
    /// <summary>
    /// This class is based on the CUETools.NET project (see http://sourceforge.net/p/cuetoolsnet/ , now located at https://github.com/gchudov/cuetools.net)
    /// The author "Grigory Chudov" explicitly gave the permission to use the source as part of the cscore source code which got licensed under the ms-pl.
    /// </summary>
    internal class CRC16 : CRCBase<ushort>
    {
        public CRC16() : base(0x8005) { }

        public override ushort CalcCheckSum(ReadOnlySpan<byte> buffer)
        {
            ushort res = 0;
            foreach (var b in buffer)
            {
                res = (ushort)((res << 8) ^ crc_table[(res >> 8) ^ b]);
            }
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override ushort CalcCheckSum(byte[] buffer, int offset, int count)
        {
            return CalcCheckSum(buffer.AsSpan(offset, count));
        }
    }
}