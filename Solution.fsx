#r "./fake/fakelib.dll"

open System
open Fake

let build config _ =
    [config |> Map.find "build:solution"]
        |> MSBuild "" "Build" ["Configuration", (config |> Map.find "build:configuration")]
        |> ignore

let clean config _ =
    [config |> Map.find "build:solution"]
        |> MSBuild "" "Clean" ["Configuration", (config |> Map.find "build:configuration")]
        |> ignore
