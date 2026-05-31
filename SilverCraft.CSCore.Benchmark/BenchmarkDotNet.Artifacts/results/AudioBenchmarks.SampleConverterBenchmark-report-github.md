```

BenchmarkDotNet v0.15.8, Linux EndeavourOS
12th Gen Intel Core i7-12700K 0.80GHz, 1 CPU, 20 logical and 12 physical cores
.NET SDK 10.0.300
  [Host]     : .NET 10.0.8 (10.0.8, 10.0.826.23019), X64 RyuJIT x86-64-v3
  DefaultJob : .NET 10.0.8 (10.0.8, 10.0.826.23019), X64 RyuJIT x86-64-v3


```
| Method                                | Count | Mean         | Error      | StdDev     | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------- |------ |-------------:|-----------:|-----------:|-------:|--------:|-------:|----------:|------------:|
| **Raw_MemoryMarshalCast**                 | **512**   |     **3.929 ns** |  **0.0328 ns** |  **0.0306 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| Raw_ManualBitConverterLoop            | 512   |   527.621 ns | 10.4449 ns | 13.2095 ns | 134.30 |    3.45 | 0.3128 |    4096 B |          NA |
| Pcm_ManualBitConverterLoopWithScaling | 512   |   567.267 ns |  4.0847 ns |  3.8208 ns | 144.39 |    1.45 | 0.3128 |    4096 B |          NA |
| Pcm_VectorizedScaling                 | 512   |     4.867 ns |  0.0869 ns |  0.0813 ns |   1.24 |    0.02 |      - |         - |          NA |
|                                       |       |              |            |            |        |         |        |           |             |
| **Raw_MemoryMarshalCast**                 | **2048**  |    **14.050 ns** |  **0.0791 ns** |  **0.0618 ns** |   **1.00** |    **0.01** |      **-** |         **-** |          **NA** |
| Raw_ManualBitConverterLoop            | 2048  | 2,059.279 ns | 23.3322 ns | 21.8249 ns | 146.57 |    1.63 | 1.2512 |   16384 B |          NA |
| Pcm_ManualBitConverterLoopWithScaling | 2048  | 2,246.479 ns | 22.2595 ns | 20.8216 ns | 159.89 |    1.59 | 1.2512 |   16384 B |          NA |
| Pcm_VectorizedScaling                 | 2048  |    24.304 ns |  0.5076 ns |  0.7440 ns |   1.73 |    0.05 |      - |         - |          NA |
