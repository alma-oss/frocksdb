# Anti-Patterns — Alma.RocksDb

Each entry is **mistake → why → fix**.

## Treating `length` as an exact count
- **Mistake:** Asserting or branching on `RocksDb.length db` as the precise number
  of stored keys.
- **Why:** It is derived from a RocksDB estimate statistic, so it may diverge from
  the true count, especially after many writes.
- **Fix:** Treat it as approximate. When you need an exact count, enumerate with
  `RocksDb.seq` and count the yielded pairs.

## Ignoring the `Result` from `connect`
- **Mistake:** Using the value from `connect`/`connectReadOnly` as if it were the
  open handle directly.
- **Why:** Both return `Result<RocksDb, exn>`; the handle lives inside `Ok`, and
  failures surface as `Error`.
- **Fix:** Pattern-match (or map/bind) and handle `Error` before any operation.

## Calling `put` with un-tupled arguments
- **Mistake:** Writing `RocksDb.put db key value` as two separate arguments.
- **Why:** `put` takes the key and value as one tuple `(key, value)`.
- **Fix:** Pass a tuple: `RocksDb.put db (key, value)` (or `(key, value) |> RocksDb.put db`).

## Forgetting to dispose the handle
- **Mistake:** Opening a store and letting the handle go out of scope without disposal.
- **Why:** The handle is `IDisposable` and owns native resources and file locks.
- **Fix:** Bind it with `use`, and in tests also delete the on-disk directory.

## Assuming a delete/clear API exists
- **Mistake:** Calling something like `RocksDb.clear` or `RocksDb.delete` to remove entries.
- **Why:** No removal function is exposed by this overlay.
- **Fix:** Restrict generated code to `connect`, `connectReadOnly`, `put`, `get`,
  `length`, and `seq`. Do not invent functions that are not in the module.

## Expecting non-string keys or values
- **Mistake:** Passing integers, byte arrays, or serialized objects directly to
  `put`/`get`.
- **Why:** The API is typed for `string` keys and `string` values only.
- **Fix:** Serialize to/deserialize from `string` at the call site before/after
  the operation.

## Writing through a read-only connection
- **Mistake:** Calling `put` on a handle opened with `connectReadOnly`.
- **Why:** A read-only store rejects writes.
- **Fix:** Use `connect` when the workload writes; reserve `connectReadOnly` for
  read-only access.

## Relying on insertion order from `seq`
- **Mistake:** Expecting `RocksDb.seq` to yield entries in the order they were written.
- **Why:** It iterates in ascending key order, not insertion order.
- **Fix:** Encode any required ordering into the keys themselves (e.g. zero-padded
  numeric prefixes).
