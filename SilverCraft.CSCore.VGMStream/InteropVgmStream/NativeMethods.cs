using System.Runtime.InteropServices;

namespace VgmStream;
internal sealed class NativeTypeNameAttribute : Attribute
{
    public NativeTypeNameAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
}
public static unsafe partial class NativeMethods
{
    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern VGMSTREAM* init_vgmstream([NativeTypeName("const char *const")] sbyte* filename);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern VGMSTREAM* init_vgmstream_from_STREAMFILE(STREAMFILE* sf);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern void reset_vgmstream(VGMSTREAM* vgmstream);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern void close_vgmstream(VGMSTREAM* vgmstream);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern int render_vgmstream2([NativeTypeName("sample_t *")] short* buffer, [NativeTypeName("int32_t")] int sample_count, VGMSTREAM* vgmstream);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern void seek_vgmstream(VGMSTREAM* vgmstream, [NativeTypeName("int32_t")] int seek_sample);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern VGMSTREAM* allocate_vgmstream(int channel_count, int looped);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern void setup_vgmstream(VGMSTREAM* vgmstream);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool vgmstream_open_stream(VGMSTREAM* vgmstream, STREAMFILE* sf, [NativeTypeName("off_t")] nint start_offset);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool vgmstream_open_stream_bf(VGMSTREAM* vgmstream, STREAMFILE* sf, [NativeTypeName("off_t")] nint start_offset, bool force_multibuffer);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern void vgmstream_force_loop(VGMSTREAM* vgmstream, int loop_flag, int loop_start_sample, int loop_end_sample);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern void vgmstream_set_loop_target(VGMSTREAM* vgmstream, int loop_target);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern void setup_vgmstream_play_state(VGMSTREAM* vgmstream);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool vgmstream_is_virtual_filename([NativeTypeName("const char *")] sbyte* filename);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_stdio_streamfile([NativeTypeName("const char *")] sbyte* filename);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_stdio_streamfile_by_file([NativeTypeName("FILE *")] IntPtr* file, [NativeTypeName("const char *")] sbyte* filename);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_buffer_streamfile(STREAMFILE* sf, [NativeTypeName("size_t")] nuint buffer_size);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_buffer_streamfile_f(STREAMFILE* sf, [NativeTypeName("size_t")] nuint buffer_size);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_wrap_streamfile(STREAMFILE* sf);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_wrap_streamfile_f(STREAMFILE* sf);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_clamp_streamfile(STREAMFILE* sf, [NativeTypeName("offv_t")] nint start, [NativeTypeName("size_t")] nuint size);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_clamp_streamfile_f(STREAMFILE* sf, [NativeTypeName("offv_t")] nint start, [NativeTypeName("size_t")] nuint size);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_io_streamfile(STREAMFILE* sf, void* data, [NativeTypeName("size_t")] nuint data_size, void* read_callback, void* size_callback);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_io_streamfile_f(STREAMFILE* sf, void* data, [NativeTypeName("size_t")] nuint data_size, void* read_callback, void* size_callback);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_io_streamfile_ex(STREAMFILE* sf, void* data, [NativeTypeName("size_t")] nuint data_size, void* read_callback, void* size_callback, void* init_callback, void* close_callback);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_io_streamfile_ex_f(STREAMFILE* sf, void* data, [NativeTypeName("size_t")] nuint data_size, void* read_callback, void* size_callback, void* init_callback, void* close_callback);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_fakename_streamfile(STREAMFILE* sf, [NativeTypeName("const char *")] sbyte* fakename, [NativeTypeName("const char *")] sbyte* fakeext);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_fakename_streamfile_f(STREAMFILE* sf, [NativeTypeName("const char *")] sbyte* fakename, [NativeTypeName("const char *")] sbyte* fakeext);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_multifile_streamfile(STREAMFILE** sfs, [NativeTypeName("size_t")] nuint sfs_size);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_multifile_streamfile_f(STREAMFILE** sfs, [NativeTypeName("size_t")] nuint sfs_size);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* open_streamfile(STREAMFILE* sf, [NativeTypeName("const char *")] sbyte* pathname);

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern STREAMFILE* reopen_streamfile(STREAMFILE* sf, [NativeTypeName("size_t")] nuint buffer_size);

    public static void close_streamfile(STREAMFILE* sf)
    {
        if (sf != null)
        {
            sf->close(sf);
        }
    }

    [return: NativeTypeName("size_t")]
    public static nuint read_streamfile([NativeTypeName("uint8_t *")] byte* dst, [NativeTypeName("offv_t")] nint offset, [NativeTypeName("size_t")] nuint length, STREAMFILE* sf)
    {
        return sf->read(sf, dst, offset, length);
    }

    [return: NativeTypeName("size_t")]
    public static nuint get_streamfile_size(STREAMFILE* sf)
    {
        return sf->get_size(sf);
    }

    [DllImport("libvgmstream.so", CallingConvention = CallingConvention.Cdecl)]
    public static extern void dump_streamfile(STREAMFILE* sf, int num);

    [NativeTypeName("#define STREAMFILE_DEFAULT_BUFFER_SIZE 0x8000")]
    public const int STREAMFILE_DEFAULT_BUFFER_SIZE = 0x8000;
}
