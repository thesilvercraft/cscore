namespace VgmStream;

public unsafe partial struct STREAMFILE
{
    [NativeTypeName("size_t (*)(struct _STREAMFILE *, uint8_t *, offv_t, size_t)")]
    public delegate* unmanaged[Cdecl]<STREAMFILE*, byte*, nint, nuint, nuint> read;

    [NativeTypeName("size_t (*)(struct _STREAMFILE *)")]
    public delegate* unmanaged[Cdecl]<STREAMFILE*, nuint> get_size;

    [NativeTypeName("offv_t (*)(struct _STREAMFILE *)")]
    public delegate* unmanaged[Cdecl]<STREAMFILE*, nint> get_offset;

    [NativeTypeName("void (*)(struct _STREAMFILE *, char *, size_t)")]
    public delegate* unmanaged[Cdecl]<STREAMFILE*, sbyte*, nuint, void> get_name;

    [NativeTypeName("struct _STREAMFILE *(*)(struct _STREAMFILE *, const char *const, size_t)")]
    public delegate* unmanaged[Cdecl]<STREAMFILE*, sbyte*, nuint, STREAMFILE*> open;

    [NativeTypeName("void (*)(struct _STREAMFILE *)")]
    public delegate* unmanaged[Cdecl]<STREAMFILE*, void> close;

    public int stream_index;
}
