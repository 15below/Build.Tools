#r "./fake/fakelib.dll"
#load "./Utils.fsx"

open System
open Fake
open Utils

let build (config: Map<string, string>) _ =
    [config.get "build:solution"]
        |> MSBuild "" "Build" ["Configuration", config.get "build:configuration"]
        |> ignore

let clean (config: Map<string, string>) _ =
    [config.get "build:solution"]
        |> MSBuild "" "Clean" ["Configuration", config.get "build:configuration"]
        |> ignore
