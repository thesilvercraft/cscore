# CSCore benchmarks

The BenchmarkDotNet project includes sample conversion, full-file FLAC decoding, and `VolumeSource` processing benchmarks. `FlacDecodeBenchmark` compares the working-tree decoder with YellowDogMan.CSCoreFork 1.2.1.2-ydm-0.1.19 in the same BenchmarkDotNet job. It reports elapsed time and managed allocations. Run Release builds with the same FLAC input on the same machine and .NET runtime.

## Run

From this directory:

```powershell
$env:CSCORE_BENCHMARK_FLAC = "C:\audio\sample.flac"
dotnet run -c Release -- --filter "*FlacDecodeBenchmark*" "*VolumeSourceBenchmark*"
```

Both FLAC methods open and decode the whole input for each benchmark operation, including decoder construction and pre-scan. Check that both methods return the same decoded byte count. BenchmarkDotNet writes reports under `BenchmarkDotNet.Artifacts`.

The volume benchmark processes a deterministic one-million-sample buffer. The FLAC file is selected with `CSCORE_BENCHMARK_FLAC`.

## Published CSCore package variant

The benchmark project also supports using a published SilverCraft package instead of the working-tree project reference:

```powershell
dotnet run -c Release -p:UseNuGetCSCore=true -p:BenchmarkLibraryVersion=0.2.0-beta -- --filter "*FlacDecodeBenchmark*"
```

For another compatible fork package, set `BenchmarkPackageId` and `BenchmarkLibraryVersion`. The YellowDogMan fork is already referenced directly for the side-by-side comparison above. Native allocations and full process memory are not included in the managed memory diagnostic; CPU time is not reported separately by these benchmarks.
