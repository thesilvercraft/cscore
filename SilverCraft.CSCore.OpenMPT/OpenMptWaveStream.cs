using OpenMPT;
using SilverAudioPlayer.Shared;

namespace SilverCraft.CSCore.OpenMPT;

public unsafe class OpenMptWaveStream : ISampleSource, ISelector, ILoop
{
    openmpt_module* infile;
    MPTVirtualStreamHelper Helper;
    private float[] _leftBuffer = [];
    private float[] _rightBuffer = [];

    public OpenMptWaveStream(Stream s)
    {
        Helper = new MPTVirtualStreamHelper(s);
        infile = NativeMethods.openmpt_module_create2(Helper.Virtual, 0, null, null, null, null, null, null, null);
        WaveFormat = new WaveFormat(48000, 32, 2, AudioEncoding.IeeeFloat);
        Length = (long)(NativeMethods.openmpt_module_get_duration_seconds(infile) * WaveFormat.SampleRate *
                        WaveFormat.Channels);
    }


    public int Read(float[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset, buffer.Length - count);

        var frameCount = count / WaveFormat.Channels;
        if (_leftBuffer.Length < frameCount)
        {
            _leftBuffer = new float[frameCount];
            _rightBuffer = new float[frameCount];
        }

        int framesRead;
        fixed (float* leftPtr = _leftBuffer, rightPtr = _rightBuffer)
        {
            framesRead = (int)NativeMethods.openmpt_module_read_float_stereo(infile, WaveFormat.SampleRate,
                (nuint)frameCount, leftPtr, rightPtr);
        }

        for (var i = 0; i < framesRead; i++)
        {
            buffer[offset + i * 2] = _leftBuffer[i];
            buffer[offset + i * 2 + 1] = _rightBuffer[i];
        }

        return framesRead * WaveFormat.Channels;
    }

    private bool isDisposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed) return;

        if (disposing)
        {
            Helper.Dispose();
        }

        NativeMethods.openmpt_module_destroy(infile);

        isDisposed = true;
    }

    ~OpenMptWaveStream()
    {
        Dispose(false);
    }

    public void SetPattern(int pattern)
    {
        _ = NativeMethods.openmpt_module_select_subsong(infile, pattern);
        Length = (long)(NativeMethods.openmpt_module_get_duration_seconds(infile) * WaveFormat.SampleRate *
                        WaveFormat.Channels);
    }

    public void EnableLoop()
    {
        _ = NativeMethods.openmpt_module_set_repeat_count(infile, -1);
    }

    public void DisableLoop()
    {
        _ = NativeMethods.openmpt_module_set_repeat_count(infile, 0);
    }

    public bool CanSeek => true;
    public WaveFormat WaveFormat { get; set; }

    public long Position
    {
        get => (long)(NativeMethods.openmpt_module_get_position_seconds(infile) * WaveFormat.SampleRate *
                      WaveFormat.Channels);
        set =>
            NativeMethods.openmpt_module_set_position_seconds(infile,
                value / (double)(WaveFormat.SampleRate * WaveFormat.Channels));
    }

    public long Length { get; set; }

    public int CurrentPattern => NativeMethods.openmpt_module_get_selected_subsong(infile);

    public int NumberOfPatterns => NativeMethods.openmpt_module_get_num_subsongs(infile);

    public bool CanLoop => true;
}