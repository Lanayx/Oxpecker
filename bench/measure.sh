#!/bin/bash
# Usage: bench/measure.sh <cpu: all|1> <seconds per endpoint> [endpoints...]
# Starts the built OxpBench server under Kestrel, drives each endpoint with wrk (keep-alive),
# and prints per-endpoint: server-side allocated bytes/request, req/s, p50 latency, gen0 GCs per 100k req.
set -euo pipefail
CPU=${1:-all}; DUR=${2:-3}; shift 2 || true
EPS=${*:-"plaintext json json-users user ctx headers composed html subroute precond"}
ROOT=$(cd "$(dirname "$0")/.." && pwd)
DLL=$ROOT/bench/OxpBench/bin/Release/net10.0/OxpBench.dll
PORT=${BENCH_PORT:-5099}
export DOTNET_ROOT=${DOTNET_ROOT:-$HOME/.dotnet}
ENVS=(BENCH_CFG=all)
if [ "$CPU" = 1 ]; then ENVS=(DOTNET_PROCESSOR_COUNT=1); THREADS=1; CONNS=8; else THREADS=2; CONNS=32; fi
env "${ENVS[@]}" BENCH_PORT=$PORT $DOTNET_ROOT/dotnet "$DLL" >/tmp/oxpbench-server.log 2>&1 &
PID=$!
trap 'kill $PID 2>/dev/null; wait $PID 2>/dev/null || true' EXIT
for i in $(seq 1 100); do curl -s -o /dev/null "http://127.0.0.1:$PORT/__stats" && break; sleep 0.2; done
url() { case $1 in
  plaintext) echo /plaintext;; json) echo /json;; json-users) echo /json-users;;
  user) echo /user/don/42;; ctx) echo /userctx/don/42;; headers) echo "/headers?q=1";;
  composed) echo /composed;; html) echo /html;; subroute) echo /api/users;; precond) echo /precond;; esac; }
for ep in $EPS; do
  U="http://127.0.0.1:$PORT$(url $ep)"
  [ "$(curl -s -o /dev/null -w '%{http_code}' -H 'X-Test: abc' -H 'If-None-Match: "v0"' "$U")" = 200 ] || { echo "ENDPOINT_FAIL $ep"; exit 1; }
  wrk -t$THREADS -c$CONNS  -d1s -H 'X-Test: abc' -H 'If-None-Match: "v0"' "$U" >/dev/null
  S1=$(curl -s "http://127.0.0.1:$PORT/__stats")
  OUT=$(wrk -t$THREADS -c$CONNS -d${DUR}s --latency -H 'X-Test: abc' -H 'If-None-Match: "v0"' "$U")
  S2=$(curl -s "http://127.0.0.1:$PORT/__stats")
  REQS=$(echo "$OUT" | awk '/requests in/ {print $1}')
  RPS=$(echo "$OUT" | awk '/Requests\/sec/ {print $2}')
  P50=$(echo "$OUT" | awk '$1=="50%" {v=$2; if (v ~ /us$/) {sub(/us/,"",v); v=v/1000} else if (v ~ /ms$/) {sub(/ms/,"",v)} else if (v ~ /s$/) {sub(/s/,"",v); v=v*1000}; print v}')
  echo "$S1 $S2 $REQS" | awk -v ep=$ep -v rps=$RPS -v p50=$P50 -v cpu=$CPU '{a=($7-$1)/$13; g=($8-$2)*100000/$13; c=($12-$6)/10/$13; printf "%s cpu=%s alloc_per_req=%.0f rps=%s p50_ms=%s cpu_us_per_req=%.2f gen0_per_100k=%.1f reqs=%d\n", ep, cpu, a, rps, p50, c, g, $13}'
done
