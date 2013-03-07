#r "./fake/fakelib.dll"

open System
open Fake

let build solution _ =
    [solution]
        |> MSBuildRelease "" "Build"
        |> ignore

let clean solution _ =
    [solution]
        |> MSBuildRelease "" "Clean" 
        |> ignore
