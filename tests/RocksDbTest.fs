module Alma.RocksDb.Test

open Expecto

open System
open System.IO
open Microsoft.Extensions.Logging
open Alma.RocksDb

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
        Db: RocksDbSharp.RocksDb option
        Path: string
        Error: string option
    }

    interface IDisposable with
        member this.Dispose() =
            match this.Db with
            | Some db -> db.Dispose()
            | _ -> ()

            Directory.delete this.Path

let connect path =
    try
        // fsharplint:disable RedundantNewKeyword
        use loggerFactory = new LoggerFactory()
        // fsharplint:enable
        let rocksDb = path |> RocksDb.connect loggerFactory |> orFail

        {
            Db = Some rocksDb
            Path = path
            Error = None
        }
    with
    | e ->
        {
            Db = None
            Path = path
            Error = Some e.Message
        }

[<Tests>]
let rocksdbTest =
    testList "Rocks Db test" [
        testCase "should put" <| fun _ ->
            use connection = connect "db/put"
            match connection with
            | { Db = Some rocksDb } ->
                ("key", "value") |> RocksDb.put rocksDb
                let value = "key" |> RocksDb.get rocksDb

                Expect.equal (Some "value") value "Value should be in the db"
            | { Error = Some message } -> Tests.skiptestf "RocksDB couldn't connect due to: %A" message
            | _ -> failtest "Failed"

        testCase "should iterate" <| fun _ ->
            use connection = connect "db/iter"
            match connection with
            | { Db = Some rocksDb } ->

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

            | { Error = Some message } -> Tests.skiptestf "RocksDB couldn't connect due to: %A" message
            | _ -> failtest "Failed"
    ]
