using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using sndfile;

namespace SilverCraft.CSCore.SndFile;

public class SndFileWaveStream : ISampleSource
{
    private unsafe sf_private_tag* _infile;
    private SF_VIRTUAL_IO _io;
    private readonly SfVirtualStreamHelper _helper;
    private readonly SF_INFO _info;
    private bool _isDisposed;

    static SndFileWaveStream()
    {
        try
        {
            NativeLibrary.SetDllImportResolver(typeof(Methods).Assembly, Methods.DllImportResolver);
        }
        catch (InvalidOperationException)
        {
        }
    }

    public unsafe SndFileWaveStream(Stream s)
    {
        ArgumentNullException.ThrowIfNull(s);

        _helper = new SfVirtualStreamHelper(s);
        _io = _helper.Virtual; 

        _info = new SF_INFO();
        _infile = Methods.sf_open_virtual(ref _io, Mode.Read, ref _info, 0);
        
        if (_infile == null)
        {
            _helper.Dispose();
            string errorMsg = Marshal.PtrToStringAnsi((IntPtr)Methods.sf_strerror(null)) ?? "Unknown error";
            throw new InvalidDataException($"SndFile error: {errorMsg}");
        }

        InitializeFormat();
    }

    public unsafe SndFileWaveStream(ref sf_private_tag* f, SF_INFO info)
    {
        if (f == null) throw new ArgumentNullException(nameof(f));
        
        _infile = f;
        _info = info;
        InitializeFormat();
    }

    private void InitializeFormat()
    {
        WaveFormat = new WaveFormat(_info.samplerate, 32, _info.channels);
        Length = _info.frames * _info.channels;

        Debug.WriteLine($"Length {Length}");
        Debug.WriteLine($"Sample rate: {_info.samplerate}Hz");
        Debug.WriteLine($"Channel count: {_info.channels}");
        Debug.WriteLine($"Format: {_info.format}");
        Debug.WriteLine($"Frames: {_info.frames}");
        Debug.WriteLine($"Seekable: {_info.seekable}");
    }

    public unsafe int Read(float[] buffer, int offset, int count)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(buffer);
        if (offset < 0 || count < 0 || offset + count > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(count), "Buffer overflow risks detected.");

        fixed (float* f = buffer)
        {
            return (int)Methods.sf_read_float(_infile, f + offset, count);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual unsafe void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            _helper?.Dispose();
        }

        if (_infile != null)
        {
            _ = Methods.sf_close(_infile);
            _infile = null;
        }

        _isDisposed = true;
    }

    ~SndFileWaveStream()
    {
        Dispose(false);
    }

    public bool CanSeek => _info.seekable != 0;
    public WaveFormat WaveFormat { get; private set; } = null!;
    public long Length { get; private set; }

    public unsafe long Position
    {
        get 
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return Methods.sf_seek(_infile, 0, Whence.Current) * _info.channels;
        }
        set
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            if (_info.seekable != 0)
            {
                Methods.sf_seek(_infile, (nint)value / _info.channels, Whence.Set);
            }
        }
    }
}