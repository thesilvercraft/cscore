using System.Runtime.InteropServices;

namespace SilverCraft.CSCore.Utils
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct HightLowConverterInt32
    {
        public HightLowConverterInt32(int value)
        {
            Low = 0;
            High = 0;
            Value = value;
        }

        [FieldOffset(0)]
        public Int32 Value;

        [FieldOffset(0)]
        public ushort Low;

        [FieldOffset(2)]
        public ushort High;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct HightLowConverterUInt32
    {
        public HightLowConverterUInt32(uint value)
        {
            Low = 0;
            High = 0;
            Value = value;
        }

        [FieldOffset(0)]
        public UInt32 Value;

        [FieldOffset(0)]
        public ushort Low;

        [FieldOffset(2)]
        public ushort High;
    }
}