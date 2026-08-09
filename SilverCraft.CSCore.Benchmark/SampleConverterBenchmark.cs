using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

namespace AudioBenchmarks;

[MemoryDiagnoser]
public class SampleConverterBenchmark
{
    [Params(512,2048)]
    public int Count;

    private float[] _sourceBuffer;
    private byte[] _targetBuffer;
    private int _offset = 0;

    [GlobalSetup]
    public void Setup()
    {
        _sourceBuffer = new float[Count / 4];
        _targetBuffer = new byte[Count];

        var rand = new Random(42);
        for (int i = 0; i < _sourceBuffer.Length; i++)
        {
            _sourceBuffer[i] = (float)(rand.NextDouble() * 2.0 - 1.0);
        }
    }


[Benchmark(Baseline = true)]
    public unsafe int Pcm_ManualAvx2()
    {
        int sampleCount = Count >> 2;
        if (sampleCount <= 0) return 0;

        int bytesRead = sampleCount << 2;

        fixed (float* srcStart = _sourceBuffer)
        fixed (byte* dstStart = _targetBuffer)
        {
            float* src = srcStart;
            int* dest = (int*)(dstStart + _offset);
            int i = 0;

            if (Avx.IsSupported)
            {
                var scaleMultiplier = Vector256.Create(2147483648f);
                int avxLimit = sampleCount & ~7;

                for (; i < avxLimit; i += 8)
                {
                    var floatInput = Unsafe.ReadUnaligned<Vector256<float>>(src + i);
                    var multiplied = Avx.Multiply(floatInput, scaleMultiplier);
                    
                    Vector256<int> intVector = Avx.ConvertToVector256Int32(multiplied);
                    
                    Unsafe.WriteUnaligned(dest + i, intVector);
                }
            }

            for (; i < sampleCount; i++)
            {
                dest[i] = (int)(src[i] * 2147483648f);
            }
        }

        return bytesRead;
    }

    [Benchmark()]
    public unsafe int Pcm_Vector256()
    {
        int sampleCount = Count >> 2;
        if (sampleCount <= 0) return 0;

        int bytesRead = sampleCount << 2;

        fixed (float* srcStart = _sourceBuffer)
        fixed (byte* dstStart = _targetBuffer)
        {
            float* src = srcStart;
            int* dest = (int*)(dstStart + _offset);
            int i = 0;

            if (Vector256.IsHardwareAccelerated)
            {
                var scaleMultiplier = Vector256.Create(2147483648f);
                int avxLimit = sampleCount & ~7; 

                for (; i < avxLimit; i += 8)
                {
                    var floatInput = Unsafe.ReadUnaligned<Vector256<float>>(src + i);
                    var multiplied = Vector256.Multiply(floatInput, scaleMultiplier);
                    
                    var intVector = Vector256.ConvertToInt32(multiplied);
                    
                    Unsafe.WriteUnaligned(dest + i, intVector);
                }
            }

            for (; i < sampleCount; i++)
            {
                dest[i] = (int)(src[i] * 2147483648f);
            }
        }

        return bytesRead;
    }
}