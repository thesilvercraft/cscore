using System;
using BenchmarkDotNet.Running;

namespace AudioBenchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<SampleConverterBenchmark>();
        }
    }
}