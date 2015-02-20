#r    @"../../../packages/FAKE/tools/fakelib.dll"
#load "./Utils.fsx"

open Fake
open Utils
open System

let run (config : Map<string, string>) _ =
    let testDlls = !! (sprintf @".\**\bin\%s\**\*.Tests.dll" (config.get "build:configuration"))
    if Seq.length testDlls > 0 then
        testDlls
        |> NUnit (fun p -> { p with ToolPath = "packages/NUnit.Runners/tools" })
