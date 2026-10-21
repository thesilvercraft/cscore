using SilverCraft.CSCore.Streams;
using Xunit;

namespace SilverCraft.CSCore.Tests;

public sealed class StreamTests
{
    [Fact]
    public void VolumeSource_AppliesVolumeWithinRequestedBufferRange()
    {
        using var source = new ArraySampleSource([1f, -0.5f, 0.25f]);
        using var volume = new VolumeSource(source) { Volume = 0.5f };
        var output = new[] { 99f, 99f, 99f, 99f, 99f };

        var read = volume.Read(output, 1, 3);

        Assert.Equal(3, read);
        Assert.Equal(new[] { 99f, 0.5f, -0.25f, 0.125f, 99f }, output);
    }

    [Fact]
    public void VolumeSource_SilencesSamplesAtZeroVolume()
    {
        using var source = new ArraySampleSource([1f, -0.5f]);
        using var volume = new VolumeSource(source) { Volume = 0f };
        var output = new[] { 99f, 99f, 99f, 99f };

        var read = volume.Read(output, 1, 2);

        Assert.Equal(2, read);
        Assert.Equal(new[] { 99f, 0f, 0f, 99f }, output);
    }

    [Theory]
    [InlineData(-0.01f)]
    [InlineData(1.01f)]
    public void VolumeSource_RejectsVolumeOutsideUnitRange(float value)
    {
        using var source = new ArraySampleSource([1f]);
        using var volume = new VolumeSource(source);

        Assert.Throws<ArgumentOutOfRangeException>(() => volume.Volume = value);
    }

    private sealed class ArraySampleSource(float[] samples) : ISampleSource
    {
        private int _position;

        public WaveFormat WaveFormat { get; } = new(44100, 32, 1, AudioEncoding.IeeeFloat);
        public long Position { get => _position; set => _position = checked((int)value); }
        public long Length => samples.Length;
        public bool CanSeek => true;

        public int Read(float[] buffer, int offset, int count)
        {
            var read = Math.Min(count, samples.Length - _position);
            Array.Copy(samples, _position, buffer, offset, read);
            _position += read;
            return read;
        }

        public void Dispose() { }
    }
}
