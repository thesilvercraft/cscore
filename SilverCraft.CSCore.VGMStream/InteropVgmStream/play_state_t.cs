namespace VgmStream;

public partial struct play_state_t
{
    [NativeTypeName("int32_t")]
    public int pad_begin_duration;

    [NativeTypeName("int32_t")]
    public int pad_begin_left;

    [NativeTypeName("int32_t")]
    public int trim_begin_duration;

    [NativeTypeName("int32_t")]
    public int trim_begin_left;

    [NativeTypeName("int32_t")]
    public int body_duration;

    [NativeTypeName("int32_t")]
    public int fade_duration;

    [NativeTypeName("int32_t")]
    public int fade_left;

    [NativeTypeName("int32_t")]
    public int fade_start;

    [NativeTypeName("int32_t")]
    public int pad_end_duration;

    [NativeTypeName("int32_t")]
    public int pad_end_start;

    [NativeTypeName("int32_t")]
    public int play_duration;

    [NativeTypeName("int32_t")]
    public int play_position;
}
