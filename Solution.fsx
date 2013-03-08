#r "./fake/fakelib.dll"

open System
open Fake

let build config _ =
    [config |> Map.find "solution"]
        |> MSBuild "" "Build" ["Configuration", (config |> Map.find "configuration")]
        |> ignore

let clean config _ =
    [config |> Map.find "solution"]
        |> MSBuild "" "Clean" ["Configuration", (config |> Map.find "configuration")]
        |> ignore
