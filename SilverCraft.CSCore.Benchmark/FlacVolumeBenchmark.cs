using BenchmarkDotNet.Attributes;
using SilverCraft.CSCore.Codecs.FLAC;
using SilverCraft.CSCore.Streams;
using SilverCraft.CSCore;

namespace AudioBenchmarks;

[MemoryDiagnoser]
public class FlacDecodeBenchmark
{
    [ParamsSource(nameof(FlacFiles))]
    public string FlacPath { get; set; } = "";

    public IEnumerable<string> FlacFiles => Environment.GetEnvironmentVariable("CSCORE_BENCHMARK_FLAC") is { Length: > 0 } path
        ? [Path.GetFullPath(path)]
        : [];

    [GlobalSetup]
    public void ValidateDecodedLength()
    {
        var workingTreeBytes = DecodeWorkingTree();
        var forkBytes = DecodeYellowDogManFork();
        if (workingTreeBytes != forkBytes)
            throw new InvalidOperationException($"Decoded output length differs: working tree={workingTreeBytes}, YellowDogMan={forkBytes}.");
    }
    [Benchmark(Baseline = true)]
    public long DecodeWorkingTree()
    {
        using var decoder = new FlacFile(FlacPath);
        var buffer = new byte[decoder.WaveFormat.BlockAlign * 4096];
        long total = 0;
        int read;
        while ((read = decoder.Read(buffer, 0, buffer.Length)) > 0)
            total += read;
        return total;
    }

    [Benchmark]
    public long DecodeYellowDogManFork()
    {
        using var decoder = new CSCore.Codecs.FLAC.FlacFile(FlacPath);
        var buffer = new byte[decoder.WaveFormat.BlockAlign * 4096];
        long total = 0;
        int read;
        while ((read = decoder.Read(buffer, 0, buffer.Length)) > 0)
            total += read;
        return total;
    }
}

[MemoryDiagnoser]
public class VolumeSourceBenchmark
{
    private const int SampleCount = 1_000_000;
    private readonly float[] _input = new float[SampleCount];
    private readonly float[] _output = new float[SampleCount];
    private readonly SyntheticSampleSource _source;
    private readonly VolumeSource _volume;

    public VolumeSourceBenchmark()
    {
        for (var i = 0; i < _input.Length; i++)
            _input[i] = MathF.Sin(i * 0.01f);
        _source = new SyntheticSampleSource(_input);
        _volume = new VolumeSource(_source) { Volume = 0.5f };
    }

    [Benchmark]
    public int ProcessSamples()
    {
        _source.Position = 0;
        return _volume.Read(_output, 0, _output.Length);
    }

    private sealed class SyntheticSampleSource(float[] data) : ISampleSource
    {
        public WaveFormat WaveFormat { get; } = new(48_000, 32, 2, AudioEncoding.IeeeFloat);
        public long Position { get; set; }
        public long Length => data.Length;
        public bool CanSeek => true;
        public int Read(float[] buffer, int offset, int count)
        {
            var read = (int)Math.Min(count, Length - Position);
            Array.Copy(data, Position, buffer, offset, read);
            Position += read;
            return read;
        }
        public void Dispose() { }
    }
}

