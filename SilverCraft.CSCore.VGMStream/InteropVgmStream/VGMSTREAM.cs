using System.Runtime.CompilerServices;

namespace VgmStream;

public unsafe partial struct VGMSTREAM
{
    public int channels;

    [NativeTypeName("int32_t")]
    public int sample_rate;

    [NativeTypeName("int32_t")]
    public int num_samples;

    public coding_t coding_type;

    public layout_t layout_type;

    public meta_t meta_type;

    public bool loop_flag;

    [NativeTypeName("int32_t")]
    public int loop_start_sample;

    [NativeTypeName("int32_t")]
    public int loop_end_sample;

    [NativeTypeName("size_t")]
    public nuint interleave_block_size;

    [NativeTypeName("size_t")]
    public nuint interleave_first_block_size;

    [NativeTypeName("size_t")]
    public nuint interleave_first_skip;

    [NativeTypeName("size_t")]
    public nuint interleave_last_block_size;

    [NativeTypeName("size_t")]
    public nuint frame_size;

    public int num_streams;

    public int stream_index;

    [NativeTypeName("size_t")]
    public nuint stream_size;

    [NativeTypeName("char[256]")]
    public _stream_name_e__FixedBuffer stream_name;

    [NativeTypeName("uint32_t")]
    public uint channel_layout;

    public bool allow_dual_stereo;

    public int format_id;

    public int codec_endian;

    public int codec_config;

    public bool codec_internal_updates;

    [NativeTypeName("int32_t")]
    public int ws_output_size;

    [NativeTypeName("int32_t")]
    public int current_sample;

    [NativeTypeName("int32_t")]
    public int samples_into_block;

    [NativeTypeName("off_t")]
    public nint current_block_offset;

    [NativeTypeName("size_t")]
    public nuint current_block_size;

    [NativeTypeName("int32_t")]
    public int current_block_samples;

    [NativeTypeName("off_t")]
    public nint next_block_offset;

    [NativeTypeName("size_t")]
    public nuint full_block_size;

    [NativeTypeName("int32_t")]
    public int loop_current_sample;

    [NativeTypeName("int32_t")]
    public int loop_samples_into_block;

    [NativeTypeName("off_t")]
    public nint loop_block_offset;

    [NativeTypeName("size_t")]
    public nuint loop_block_size;

    [NativeTypeName("int32_t")]
    public int loop_block_samples;

    [NativeTypeName("off_t")]
    public nint loop_next_block_offset;

    [NativeTypeName("size_t")]
    public nuint loop_full_block_size;

    public bool hit_loop;

    public VGMSTREAMCHANNEL* ch;

    public VGMSTREAMCHANNEL* loop_ch;

    public void* start_vgmstream;

    public VGMSTREAMCHANNEL* start_ch;

    public void* mixer;

    public void* codec_data;

    public void* layout_data;

    public bool config_enabled;

    public play_config_t config;

    public play_state_t pstate;

    public int loop_count;

    public int loop_target;

    public void* tmpbuf;

    [NativeTypeName("size_t")]
    public nuint tmpbuf_size;

    public void* decode_state;

    public void* seek_table;

    [InlineArray(256)]
    public partial struct _stream_name_e__FixedBuffer
    {
        public sbyte e0;
    }
}
