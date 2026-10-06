# Examples — Alma.RocksDb

This file is the single source of truth for all example code in this skill.
Examples are ordered by increasing complexity and each is self-contained.

## Basic Usage

```fsharp
open Microsoft.Extensions.Logging
open Alma.RocksDb

use loggerFactory = LoggerFactory.Create(fun b -> b.AddConsole() |> ignore)

match "data/cache-instance" |> RocksDb.connect loggerFactory with
| Ok db ->
    use db = db
    ("greeting", "hello") |> RocksDb.put db

    match "greeting" |> RocksDb.get db with
    | Some value -> printfn "value = %s" value
    | None -> printfn "missing key"
| Error err ->
    eprintfn "failed to open store: %A" err
```

## Realistic Open With Result Handling

```fsharp
open Microsoft.Extensions.Logging
open Alma.RocksDb

/// Opens a writable store and runs an action, surfacing any open error.
let withStore (loggerFactory: ILoggerFactory) path action =
    match path |> RocksDb.connect loggerFactory with
    | Ok db ->
        use db = db
        action db |> Ok
    | Error err -> Error err

let storeCount (loggerFactory: ILoggerFactory) =
    "data/example-api"
    |> fun path -> withStore loggerFactory path (fun db ->
        ("k1", "v1") |> RocksDb.put db
        ("k2", "v2") |> RocksDb.put db
        db |> RocksDb.length)
```

## Iterating Entries

```fsharp
open Alma.RocksDb

// Keys are zero-padded so ascending key order is also numeric order.
let writeOrdered (db: RocksDbSharp.RocksDb) =
    [ 1 .. 10 ]
    |> List.iter (fun i -> (sprintf "%02i" i, string i) |> RocksDb.put db)

let readAllInOrder (db: RocksDbSharp.RocksDb) =
    db
    |> RocksDb.seq
    |> Seq.toList   // [ ("01", "1"); ("02", "2"); ... ("10", "10") ]
```

## Read-Only Access

```fsharp
open Microsoft.Extensions.Logging
open Alma.RocksDb

let readValue (loggerFactory: ILoggerFactory) path key =
    match path |> RocksDb.connectReadOnly loggerFactory with
    | Ok db ->
        use db = db
        key |> RocksDb.get db
    | Error _ -> None
```

## Integration With Logging

```fsharp
open Microsoft.Extensions.Logging
open Alma.RocksDb

// The library logs connection steps under the "RocksDb" category at Debug level.
use loggerFactory =
    LoggerFactory.Create(fun builder ->
        builder
            .SetMinimumLevel(LogLevel.Debug)
            .AddConsole()
        |> ignore)

let openWithLogging path =
    path |> RocksDb.connect loggerFactory
```

## Test With Expecto

```fsharp
open System.IO
open Expecto
open Microsoft.Extensions.Logging
open Alma.RocksDb

[<Tests>]
let tests =
    testList "store" [
        testCase "put then get returns the stored value" <| fun _ ->
            let path = "db/put-then-get"
            use loggerFactory = new LoggerFactory()

            match path |> RocksDb.connect loggerFactory with
            | Ok db ->
                use db = db
                ("key", "value") |> RocksDb.put db
                let actual = "key" |> RocksDb.get db
                Expect.equal actual (Some "value") "value should be readable"
            | Error err -> failtestf "could not open store: %A" err

            Directory.Delete(path, true)
    ]
```

## Full Workflow

```fsharp
open System.IO
open Microsoft.Extensions.Logging
open Alma.RocksDb

/// Open, populate, count via iteration, read back, then clean up.
let runWorkflow () =
    let path = "data/demo-system"
    use loggerFactory = LoggerFactory.Create(fun b -> b.AddConsole() |> ignore)

    match path |> RocksDb.connect loggerFactory with
    | Ok db ->
        use db = db

        [ "alpha", "1"; "bravo", "2"; "charlie", "3" ]
        |> List.iter (RocksDb.put db)

        let exactCount = db |> RocksDb.seq |> Seq.length
        let bravo = "bravo" |> RocksDb.get db

        printfn "count=%d bravo=%A" exactCount bravo
    | Error err -> eprintfn "open failed: %A" err

    Directory.Delete(path, true)
```
