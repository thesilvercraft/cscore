namespace SilverCraft.CSCore.Tests;

public sealed class TestSampleSource : ISampleSource
{
    private readonly float[] _samples;
    private int _position;

    public TestSampleSource(float[] samples, WaveFormat? waveFormat = null)
    {
        _samples = samples;
        WaveFormat = waveFormat ?? new WaveFormat(44100, 32, 1, AudioEncoding.IeeeFloat);
    }

    public WaveFormat WaveFormat { get; }
    public long Position
    {
        get => _position;
        set => _position = checked((int)value);
    }
    public long Length => _samples.Length;
    public bool CanSeek => true;

    public int Read(float[] buffer, int offset, int count)
    {
        var read = Math.Min(count, _samples.Length - _position);
        if (read <= 0) return 0;
        Array.Copy(_samples, _position, buffer, offset, read);
        _position += read;
        return read;
    }

    public void Dispose() { }
}

public sealed class TestWaveSource : IWaveSource
{
    private readonly byte[] _data;
    private int _position;

    public TestWaveSource(byte[] data, WaveFormat waveFormat)
    {
        _data = data;
        WaveFormat = waveFormat;
    }

    public WaveFormat WaveFormat { get; }
    public long Position
    {
        get => _position;
        set => _position = checked((int)value);
    }
    public long Length => _data.Length;
    public bool CanSeek => true;

    public int Read(byte[] buffer, int offset, int count)
    {
        var read = Math.Min(count, _data.Length - _position);
        if (read <= 0) return 0;
        Array.Copy(_data, _position, buffer, offset, read);
        _position += read;
        return read;
    }

    public void Dispose() { }
}
