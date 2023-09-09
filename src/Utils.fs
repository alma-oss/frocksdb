namespace Alma.RocksDb

[<AutoOpen>]
module internal Utils =

    let tee f a =
        f a
        a

    [<RequireQualifiedAccess>]
    module Directory =
        open System.IO

        let ensure dir =
            if dir |> Directory.Exists |> not then
                Directory.CreateDirectory(dir) |> ignore
