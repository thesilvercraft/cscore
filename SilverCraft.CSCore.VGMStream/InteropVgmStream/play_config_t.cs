namespace VgmStream;

public partial struct play_config_t
{
    public bool config_set;

    public bool play_forever;

    public bool ignore_loop;

    public bool force_loop;

    public bool really_force_loop;

    public bool ignore_fade;

    public double loop_count;

    [NativeTypeName("int32_t")]
    public int pad_begin;

    [NativeTypeName("int32_t")]
    public int trim_begin;

    [NativeTypeName("int32_t")]
    public int body_time;

    [NativeTypeName("int32_t")]
    public int trim_end;

    public double fade_delay;

    public double fade_time;

    [NativeTypeName("int32_t")]
    public int pad_end;

    public double pad_begin_s;

    public double trim_begin_s;

    public double body_time_s;

    public double trim_end_s;

    public double pad_end_s;

    public bool pad_begin_set;

    public bool trim_begin_set;

    public bool body_time_set;

    public bool loop_count_set;

    public bool trim_end_set;

    public bool fade_delay_set;

    public bool fade_time_set;

    public bool pad_end_set;

    public bool is_txtp;

    public bool is_mini_txtp;
}
