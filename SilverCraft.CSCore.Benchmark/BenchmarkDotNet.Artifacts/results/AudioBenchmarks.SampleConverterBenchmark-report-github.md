```

BenchmarkDotNet v0.15.8, Linux CachyOS
AMD Ryzen Z1 Extreme 3,242.06GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.110
  [Host]     : .NET 10.0.10 (10.0.10, 42.42.42.42424), X64 RyuJIT x86-64-v4
  DefaultJob : .NET 10.0.10 (10.0.10, 42.42.42.42424), X64 RyuJIT x86-64-v4


```
| Method         | Count | Mean      | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
|--------------- |------ |----------:|----------:|----------:|------:|--------:|----------:|------------:|
| **Pcm_ManualAvx2** | **512**   |  **5.897 ns** | **0.0690 ns** | **0.0645 ns** |  **1.00** |    **0.02** |         **-** |          **NA** |
| Pcm_Vector256  | 512   |  7.340 ns | 0.0602 ns | 0.0563 ns |  1.24 |    0.02 |         - |          NA |
|                |       |           |           |           |       |         |           |             |
| **Pcm_ManualAvx2** | **2048**  | **20.929 ns** | **0.0442 ns** | **0.0413 ns** |  **1.00** |    **0.00** |         **-** |          **NA** |
| Pcm_Vector256  | 2048  | 29.060 ns | 0.2070 ns | 0.1835 ns |  1.39 |    0.01 |         - |          NA |
