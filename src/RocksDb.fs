namespace Alma.RocksDb

[<RequireQualifiedAccess>]
module RocksDb =
    open Microsoft.Extensions.Logging
    open RocksDbSharp

    type private OpenMode =
        | Normal
        | ReadOnly

    let private openConnection mode (loggerFactory: ILoggerFactory) (path: string) =
        try
            let logger = loggerFactory.CreateLogger("RocksDb")
            logger.LogDebug("Ensure directory {path}", path)
            Directory.ensure path
            logger.LogDebug("Directory {path} is {state}.", path, (if path |> System.IO.Directory.Exists then "ready" else "not there"))

            logger.LogDebug("Prepare options")
            let options =
                DbOptions()
                    .SetCreateIfMissing(true)
                    .EnableStatistics()
            logger.LogDebug("Open connection at {path}", path)

            match mode with
            | Normal -> RocksDb.Open(options, path)
            | ReadOnly -> RocksDb.OpenReadOnly(options, path, false)
            |> tee (fun _ -> logger.LogDebug("Connected."))
            |> Ok
        with
        | e -> Error e

    let connect (loggerFactory: ILoggerFactory) (path: string) =
        openConnection Normal (loggerFactory: ILoggerFactory) (path: string)

    let connectReadOnly (loggerFactory: ILoggerFactory) (path: string) =
        openConnection ReadOnly (loggerFactory: ILoggerFactory) (path: string)

    let put (db: RocksDb) (key: string, value: string) =
        db.Put(key, value)

    let get (db: RocksDb) (key: string): string option =
        match db.Get(key) with
        | null -> None
        | value -> Some value

    let length (db: RocksDb) =
        db.GetProperty("rocksdb.estimate-num-keys")
        |> int

    let seq (db: RocksDb) =
        seq {
            use iter = db.NewIterator()
            iter.SeekToFirst() |> ignore

            while iter.Valid() do
                yield iter.StringKey(), iter.StringValue()
                iter.Next() |> ignore
        }
