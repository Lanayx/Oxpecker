# Benchmarks

Shared endpoints are in `Shared/Endpoints.fs`. Both benches use them.

Build first (Release):

```sh
dotnet build bench/OxpMicro -c Release
dotnet build bench/OxpBench -c Release
```

## OxpMicro: in-process, no network

Allocated bytes per call (deterministic, takes a few seconds):

```sh
dotnet bench/OxpMicro/bin/Release/net10.0/OxpMicro.dll --alloc            # all endpoints
dotnet bench/OxpMicro/bin/Release/net10.0/OxpMicro.dll --alloc precond    # only some
```

Time and allocations with BenchmarkDotNet (run from `bench/`, so artifacts land in the ignored `BenchmarkDotNet.Artifacts/`):

```sh
cd bench && dotnet OxpMicro/bin/Release/net10.0/OxpMicro.dll --filter '*Precond*'
```

## OxpBench: Kestrel + wrk

Needs `wrk`. Starts the server on port 5099 (`BENCH_PORT` overrides it) and runs each endpoint:

```sh
bench/measure.sh all 3                   # all CPUs, 3 s per endpoint, all endpoints
bench/measure.sh 1 3 plaintext precond   # 1 CPU, only these endpoints
```

The runtime comes from `$DOTNET_ROOT` (default `~/.dotnet`). Each line shows `alloc_per_req`, `rps`, `p50_ms`, `cpu_us_per_req` and `gen0_per_100k`. Run on a quiet machine: Mac timings are noisy, but allocations are stable.
