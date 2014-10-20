#r "./fake/fakelib.dll"
#load "./Utils.fsx"

open Fake
open Utils
open System

let run (config : Map<string, string>) _ =
    let testDlls = !! (sprintf @".\**\bin\%s\**\*.Tests.dll" (config.get "build:configuration"))
    if Seq.length testDlls > 0 then
        match packageType with
        | NuGet -> ensureNunitRunner config
        | Paket -> ()

        testDlls
        |> NUnit id
