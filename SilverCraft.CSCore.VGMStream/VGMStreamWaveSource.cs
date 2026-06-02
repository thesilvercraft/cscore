using System.Runtime.InteropServices;
using VgmStream; 

namespace SilverCraft.CSCore.VGMStream;

/// <summary>
/// Represents a stream wrapper for reading VGM format audio data from a file.
/// This class implements <see cref="IWaveSource"/> to provide an interface for reading streaming audio content.
/// </summary>
public unsafe class VGMStreamWaveSource : IWaveSource
{
    private VGMSTREAM* _vgmstream;
    private STREAMFILE* _streamfile;
    private long _positionSamples; // Tracked strictly in single-channel sample frames

    public  VGMStreamWaveSource(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) 
            throw new ArgumentNullException(nameof(filePath));
        if (!File.Exists(filePath)) 
            throw new FileNotFoundException("Target audio file not found.", filePath);
        var pathBytes = System.Text.Encoding.UTF8.GetBytes(filePath + "\0");
        fixed (byte* pPath = pathBytes)
        {
            _streamfile = NativeMethods.open_stdio_streamfile((sbyte*)pPath);
        }

        if (_streamfile == null)
            throw new InvalidOperationException("vgmstream failed to allocate its native standard file handle.");
        _vgmstream = NativeMethods.init_vgmstream_from_STREAMFILE(_streamfile);
        if (_vgmstream == null)
        {
            NativeMethods.close_streamfile(_streamfile);
            throw new InvalidDataException("vgmstream failed to parse or format audio.");
        }
        int channels = _vgmstream->channels;
        int sampleRate = _vgmstream->sample_rate;
        WaveFormat = new WaveFormat(sampleRate, 16, channels);
        CanSeek = true;
        _positionSamples = 0;
    }

    public int Read(byte[] buffer, int offset, int count)
    {
        if (_vgmstream == null) throw new ObjectDisposedException(nameof(VGMStreamWaveSource));

        int bytesPerFrame = 2 * WaveFormat.Channels; 
        int framesToRender = count / bytesPerFrame;
        if (framesToRender <= 0) return 0;

        if (_vgmstream->loop_flag == false && _positionSamples >= _vgmstream->num_samples)
            return 0;

        if (_vgmstream->loop_flag == false && (_positionSamples + framesToRender) > _vgmstream->num_samples)
        {
            framesToRender = (int)(_vgmstream->num_samples - _positionSamples);
        }
        var destByteSpan = buffer.AsSpan(offset, framesToRender * bytesPerFrame);
        var destShortSpan = MemoryMarshal.Cast<byte, short>(destByteSpan);
        var returnedSamples = 0;
        fixed (short* pDest = destShortSpan)
        {
            returnedSamples=NativeMethods.render_vgmstream2(pDest, framesToRender, _vgmstream);
        }
        _positionSamples += returnedSamples;
        return destByteSpan.Length;
    }

    public void Seek(long bytePosition)
    {
        if (!CanSeek) throw new NotSupportedException("Underlying source stream does not support seeking.");
        ObjectDisposedException.ThrowIf(_vgmstream==null, typeof(VGMStreamWaveSource));
        var bytesPerFrame = 2 * WaveFormat.Channels;
        var targetSample = (int)(bytePosition / bytesPerFrame);
        
        NativeMethods.seek_vgmstream(_vgmstream, targetSample);
        _positionSamples = targetSample;
    }

    public bool CanSeek { get; }
    public WaveFormat WaveFormat { get; }

    public long Position
    {
        get => _positionSamples * (2 * WaveFormat.Channels);
        set => Seek(value);
    }

    public long Length => (long)_vgmstream->num_samples * (2 * WaveFormat.Channels);
    
    public void Dispose()
    {
        if (_vgmstream != null)
        {
            NativeMethods.close_vgmstream(_vgmstream);
            _vgmstream = null;
        }

        if (_streamfile != null)
        {
            NativeMethods.close_streamfile(_streamfile);
            _streamfile = null;
        }
        
        GC.SuppressFinalize(this);
    }

    ~VGMStreamWaveSource()
    {
        Dispose();
    }
}