---
name: frocksdb
description: >-
  Use whenever generating or reviewing F# code that opens an embedded RocksDB
  key-value store or calls the Alma.RocksDb module: RocksDb.connect,
  RocksDb.connectReadOnly, RocksDb.put, RocksDb.get, RocksDb.length, RocksDb.seq.
  Trigger on mentions of Alma.RocksDb, RocksDbSharp, embedded key-value store,
  persistent string store, ILoggerFactory-based connection, Result-returning
  database open, or iterating keys in sorted order.
---

# F-Rocksdb

Library: [alma-oss/frocksdb](https://github.com/alma-oss/frocksdb)
NuGet: `Alma.RocksDb`

## Purpose
`Alma.RocksDb` is an F# functional overlay over the `RocksDbSharp` bindings for
RocksDB, an embedded persistent key-value store. It exposes a small,
qualified-access `RocksDb` module that wraps connection, read/write, count, and
iteration as plain F# functions operating on string keys and string values.

## When to Use
- Generating or reviewing F# that persists string key/value pairs locally via RocksDB.
- Opening a store, reading/writing entries, counting, or iterating all entries in key order.
- Wiring a RocksDB connection through `Microsoft.Extensions.Logging`.

## When NOT to Use
- Non-F# callers, or code that uses `RocksDbSharp` directly without this overlay.
- Scenarios needing column families, batches, transactions, prefix seeks, byte-array
  values, or deletion — these are not surfaced by this library.
- Environments without the native RocksDB library installed (it is a hard runtime requirement).

## Main Concepts
- **`RocksDb` module** — `RequireQualifiedAccess` module holding every public function.
- **`connect`** — opens (creating if missing) a writable store; returns a `Result`.
- **`connectReadOnly`** — opens an existing store in read-only mode; returns a `Result`.
- **`put`** — writes one entry; takes the key and value as a single tuple argument.
- **`get`** — reads one entry as `string option` (`None` when the key is absent).
- **`length`** — an approximate key count derived from a RocksDB statistic.
- **`seq`** — lazily yields all `(key, value)` pairs in ascending key order.
- **`RocksDb` handle** — the underlying `RocksDbSharp.RocksDb`, which is `IDisposable`.

## Related Libraries
- `RocksDbSharp` — the .NET binding wrapped by this overlay.
- `Microsoft.Extensions.Logging` — `ILoggerFactory` is required to open a connection.

## Keywords for Search
Alma.RocksDb, RocksDb.connect, RocksDb.connectReadOnly, RocksDb.put, RocksDb.get,
RocksDb.length, RocksDb.seq, RocksDbSharp, embedded key-value store, F# RocksDB,
ILoggerFactory, Result open database, sorted iteration, string store.

## Reference Files
- For composition principles, recommended API usage, error handling, disposal,
  logging integration, naming, and testing, read `references/preferred-patterns.md`.
- For known pitfalls, incorrect assumptions, and the phantom-API trap, read
  `references/anti-patterns.md`.
- For worked, self-contained code examples ordered by complexity, read
  `references/examples.md`.
