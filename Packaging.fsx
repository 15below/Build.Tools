#r "./fake/fakelib.dll"

open System
open Fake

let private nuget = "./nuget/nuget.exe"
let private configs = "./**/packages.config"

let restore tools _ =
    !! configs
        |> Seq.iter (RestorePackage (fun x -> 
            { x with ToolPath = tools @@ nuget }))
