# Preferred Patterns — Alma.RocksDb

## Core Principles
- Treat the module as a thin functional layer: data flows through `RocksDb.*`
  functions, with the open `RocksDb` handle passed as the first argument.
- Keep keys and values as `string`; the API does not accept other types.
- Pipe-friendly call style fits naturally, e.g. `key |> RocksDb.get db`.

## Recommended API Usage
- Open a writable store with `connect`, or an existing store with `connectReadOnly`.
  Both require an `ILoggerFactory` and a filesystem path; `connect` creates the
  directory and the store when missing.
- Write with `put`, passing key and value as a **single tuple** `(key, value)`.
- Read with `get`, which returns `string option`; branch on `Some`/`None`.
- Enumerate everything with `seq`, which yields `(key, value)` pairs in ascending
  key order — rely on this ordering rather than re-sorting.
- See `examples.md` → Basic Usage for the minimal open/put/get flow, and
  `examples.md` → Iterating Entries for ordered enumeration.

## Error Handling
- `connect` and `connectReadOnly` return `Result<RocksDb, exn>`; any failure to
  open is captured as `Error`. Always pattern-match (or otherwise handle both
  cases) before using the handle — never assume success. See `examples.md` →
  Realistic Open With Result Handling.

## Composition
- Build small helpers that accept the open handle and compose `get`/`put`/`seq`
  with ordinary F# (`Option`, `Seq`, pipelines). Keep the handle threaded
  explicitly rather than capturing it in global state.

## Integration with Other Libraries
- Provide the `ILoggerFactory` from `Microsoft.Extensions.Logging`; the library
  emits debug logs during connection under the `"RocksDb"` category. See
  `examples.md` → Integration With Logging.

## Naming Conventions
- Call functions through the qualified module name (`RocksDb.put`, `RocksDb.get`)
  since the module is `RequireQualifiedAccess`.
- Name the open handle descriptively (e.g. `db`, `cacheInstance`) and pass it as
  the first argument to every operation.

## Disposal
- The open handle is `IDisposable`. Bind it with `use` (or otherwise dispose it)
  so the native store is released; in tests also clean up the on-disk directory.
  See `examples.md` → Test With Expecto.

## Testing Recommendations
- Use Expecto. Open each store under a unique temporary path, assert via
  `Expect.equal`, and dispose the handle plus delete the directory between tests
  to keep runs isolated. See `examples.md` → Test With Expecto.
