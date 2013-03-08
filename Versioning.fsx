#r "./fake/fakelib.dll"

open System
open Fake

let private updateAssemblyInfo config file =
    trace file
    ()

let update config _ =
    !+ "./**/AssemblyInfo.cs"
    ++ "./**/AssemblyInfo.fs"
        |> Scan
        |> Seq.iter (updateAssemblyInfo config)