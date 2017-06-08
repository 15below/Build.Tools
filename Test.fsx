#r    @"../../../packages/FAKE/tools/FakeLib.dll"
#load "./Utils.fsx"

open Fake
open Utils
open System
open Fake.Testing.NUnit3

let run (config : Map<string, string>) _ =
    let testDlls = !! (sprintf @".\**\bin\%s\**\*.Tests.dll" (config.get "build:configuration"))
    if Seq.length testDlls > 0 then
        testDlls
        |> NUnit3 id
