using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace VgmStream;

public unsafe partial struct VGMSTREAMCHANNEL
{
    public STREAMFILE* streamfile;

    [NativeTypeName("off_t")]
    public nint channel_start_offset;

    [NativeTypeName("off_t")]
    public nint offset;

    [NativeTypeName("__AnonymousRecord_vgmstream_L103_C5")]
    public _Anonymous1_e__Union Anonymous1;

    [NativeTypeName("__AnonymousRecord_vgmstream_L110_C5")]
    public _Anonymous2_e__Union Anonymous2;

    [NativeTypeName("__AnonymousRecord_vgmstream_L114_C5")]
    public _Anonymous3_e__Union Anonymous3;

    [NativeTypeName("__AnonymousRecord_vgmstream_L118_C5")]
    public _Anonymous4_e__Union Anonymous4;

    [NativeTypeName("__AnonymousRecord_vgmstream_L122_C5")]
    public _Anonymous5_e__Union Anonymous5;

    [NativeTypeName("__AnonymousRecord_vgmstream_L131_C5")]
    public _Anonymous6_e__Union Anonymous6;

    [NativeTypeName("off_t")]
    public nint ws_frame_header_offset;

    public int ws_samples_left_in_frame;

    [NativeTypeName("struct g72x_state")]
    public void* g72x_state;

    [NativeTypeName("uint16_t")]
    public ushort adx_xor;

    [NativeTypeName("uint16_t")]
    public ushort adx_mult;

    [NativeTypeName("uint16_t")]
    public ushort adx_add;

    [UnscopedRef]
    public Span<short> adpcm_coef
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return Anonymous1.adpcm_coef;
        }
    }

    [UnscopedRef]
    public Span<short> vadpcm_coefs
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return Anonymous1.vadpcm_coefs;
        }
    }

    [UnscopedRef]
    public Span<int> adpcm_coef_3by32
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return Anonymous1.adpcm_coef_3by32;
        }
    }

    [UnscopedRef]
    public ref short adpcm_history1_16
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return ref Anonymous2.adpcm_history1_16;
        }
    }

    [UnscopedRef]
    public ref int adpcm_history1_32
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return ref Anonymous2.adpcm_history1_32;
        }
    }

    [UnscopedRef]
    public ref short adpcm_history2_16
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return ref Anonymous3.adpcm_history2_16;
        }
    }

    [UnscopedRef]
    public ref int adpcm_history2_32
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return ref Anonymous3.adpcm_history2_32;
        }
    }

    [UnscopedRef]
    public ref short adpcm_history3_16
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return ref Anonymous4.adpcm_history3_16;
        }
    }

    [UnscopedRef]
    public ref int adpcm_history3_32
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return ref Anonymous4.adpcm_history3_32;
        }
    }

    [UnscopedRef]
    public ref short adpcm_history4_16
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return ref Anonymous5.adpcm_history4_16;
        }
    }

    [UnscopedRef]
    public ref int adpcm_history4_32
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return ref Anonymous5.adpcm_history4_32;
        }
    }

    [UnscopedRef]
    public ref int adpcm_step_index
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return ref Anonymous6.adpcm_step_index;
        }
    }

    [UnscopedRef]
    public ref int adpcm_scale
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return ref Anonymous6.adpcm_scale;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public partial struct _Anonymous1_e__Union
    {
        [FieldOffset(0)]
        [NativeTypeName("int16_t[16]")]
        public _adpcm_coef_e__FixedBuffer adpcm_coef;

        [FieldOffset(0)]
        [NativeTypeName("int16_t[128]")]
        public _vadpcm_coefs_e__FixedBuffer vadpcm_coefs;

        [FieldOffset(0)]
        [NativeTypeName("int32_t[96]")]
        public _adpcm_coef_3by32_e__FixedBuffer adpcm_coef_3by32;

        [InlineArray(16)]
        public partial struct _adpcm_coef_e__FixedBuffer
        {
            public short e0;
        }

        [InlineArray(128)]
        public partial struct _vadpcm_coefs_e__FixedBuffer
        {
            public short e0;
        }

        [InlineArray(96)]
        public partial struct _adpcm_coef_3by32_e__FixedBuffer
        {
            public int e0;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public partial struct _Anonymous2_e__Union
    {
        [FieldOffset(0)]
        [NativeTypeName("int16_t")]
        public short adpcm_history1_16;

        [FieldOffset(0)]
        [NativeTypeName("int32_t")]
        public int adpcm_history1_32;
    }

    [StructLayout(LayoutKind.Explicit)]
    public partial struct _Anonymous3_e__Union
    {
        [FieldOffset(0)]
        [NativeTypeName("int16_t")]
        public short adpcm_history2_16;

        [FieldOffset(0)]
        [NativeTypeName("int32_t")]
        public int adpcm_history2_32;
    }

    [StructLayout(LayoutKind.Explicit)]
    public partial struct _Anonymous4_e__Union
    {
        [FieldOffset(0)]
        [NativeTypeName("int16_t")]
        public short adpcm_history3_16;

        [FieldOffset(0)]
        [NativeTypeName("int32_t")]
        public int adpcm_history3_32;
    }

    [StructLayout(LayoutKind.Explicit)]
    public partial struct _Anonymous5_e__Union
    {
        [FieldOffset(0)]
        [NativeTypeName("int16_t")]
        public short adpcm_history4_16;

        [FieldOffset(0)]
        [NativeTypeName("int32_t")]
        public int adpcm_history4_32;
    }

    [StructLayout(LayoutKind.Explicit)]
    public partial struct _Anonymous6_e__Union
    {
        [FieldOffset(0)]
        public int adpcm_step_index;

        [FieldOffset(0)]
        public int adpcm_scale;
    }
}
