#r    @"../../../packages/FAKE/tools/FakeLib.dll"

open Fake
open System
open System.IO
open System.Text.RegularExpressions
open Microsoft.FSharp.Collections

type Map<'Key,'Value when 'Key : comparison> with
    member this.get (name: 'Key) =
        this |> Map.find name


let isPullRequest (config : Map<string, string>) =
    match config.get "versioning:branch", config.get "utils:pullrequestbranchspec" with
    | null, null
    | null, _
    | _, null -> false
    | branch, spec ->
        Regex.IsMatch (branch, spec)
