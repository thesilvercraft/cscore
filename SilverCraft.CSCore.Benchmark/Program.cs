using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace AudioBenchmarks
{
    [MemoryDiagnoser]
    public class SampleConverterBenchmark
    {
        // Standard audio buffer sizes (frames * channels)
        [Params(512, 2048)]
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
        public int Raw_MemoryMarshalCast()
        {
            int sampleCount = Count >> 2;
            if (sampleCount <= 0) return 0;

            int bytesRead = sampleCount << 2;

            ReadOnlySpan<float> floatSpan = new ReadOnlySpan<float>(_sourceBuffer, 0, sampleCount);
            ReadOnlySpan<byte> sourceBytes = MemoryMarshal.AsBytes(floatSpan);
            Span<byte> destBytes = new Span<byte>(_targetBuffer, _offset, bytesRead);

            sourceBytes.CopyTo(destBytes);
            return bytesRead;
        }

        [Benchmark]
        public int Raw_ManualBitConverterLoop()
        {
            int read = Count / 4;
            var bufferOffset = _offset;

            for (var i = 0; i < read; i++)
            {
                var bytes = BitConverter.GetBytes(_sourceBuffer[i]);

                _targetBuffer[bufferOffset++] = bytes[0];
                _targetBuffer[bufferOffset++] = bytes[1];
                _targetBuffer[bufferOffset++] = bytes[2];
                _targetBuffer[bufferOffset++] = bytes[3];
            }

            return read * 4;
        }


        [Benchmark]
        public int Pcm_ManualBitConverterLoopWithScaling()
        {
            int read = Count / 4;
            var bufferOffset = _offset;

            for (var i = 0; i < read; i++)
            {
                var value = (int)(_sourceBuffer[i] * int.MaxValue);
                var bytes = BitConverter.GetBytes(value);

                _targetBuffer[bufferOffset++] = bytes[0];
                _targetBuffer[bufferOffset++] = bytes[1];
                _targetBuffer[bufferOffset++] = bytes[2];
                _targetBuffer[bufferOffset++] = bytes[3];
            }

            return read * 4;
        }

        [Benchmark]
        public unsafe int Pcm_VectorizedScaling()
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
                    var scaleMultiplier = Vector256.Create((float)int.MaxValue);
                    int avxLimit = sampleCount & ~7; // 8 floats per vector

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
                    dest[i] = (int)(src[i] * int.MaxValue);
                }
            }

            return bytesRead;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<SampleConverterBenchmark>();
        }
    }
}