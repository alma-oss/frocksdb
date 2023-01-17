module Lmc.RocksDb.Test

open Expecto

open System
open System.IO
open Microsoft.Extensions.Logging
open Lmc.RocksDb

let orFail = function
    | Ok ok -> ok
    | Error err -> failtestf "%A" err

[<RequireQualifiedAccess>]
module Directory =
    let delete path =
        let di = DirectoryInfo path

        di.GetFiles()
        |> Seq.iter (fun file -> file.Delete())

        di.GetDirectories()
        |> Seq.iter (fun dir -> dir.Delete(true))

type Connection =
    {
        Db: RocksDbSharp.RocksDb
        Path: string
    }

    interface IDisposable with
        member this.Dispose() =
            this.Db.Dispose()

            Directory.delete this.Path

let connect path =
    use loggerFactory = new LoggerFactory()
    let rocksDb = path |> RocksDb.connect loggerFactory |> orFail

    {
        Db = rocksDb
        Path = path
    }

[<Tests>]
let rocksdbTest =
    testList "Rocks Db test" [
        testCase "should put" <| fun _ ->
            let { Db = rocksDb } = connect "db/put"

            ("key", "value") |> RocksDb.put rocksDb
            let value = "key" |> RocksDb.get rocksDb

            Expect.equal (Some "value") value "Value should be in the db"

        testCase "should iterate" <| fun _ ->
            let { Db = rocksDb } = connect "db/iter"

            let data =
                [ 1 .. 10 ]
                |> List.map (fun i -> sprintf "%02i" i, $"{i}")

            data
            |> List.iter (RocksDb.put rocksDb)

            let actual =
                rocksDb
                |> RocksDb.seq
                |> Seq.toList

            Expect.equal data.Length (rocksDb |> RocksDb.length) "There should be the same amount of values"
            Expect.equal data actual "Values should be in the db"
    ]
