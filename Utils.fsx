#r    "./fake/fakelib.dll"

open Fake
open System
open Microsoft.FSharp.Collections

let nunitRunners = "./NUnit.Runners/tools"
let specFlowRunners = "./SpecFlow/tools"
let nuget = "./nuget/nuget.exe"

type Map<'Key,'Value when 'Key : comparison> with
    member this.get (name: 'Key) =
        this |> Map.find name

let ensureNunitRunner (config : Map<string, string>) =
  if not (directoryExists <| config.get "core:tools" @@ nunitRunners) then
     let args =
         sprintf "install NUnit.Runners -ExcludeVersion -OutputDirectory \"%s\""
             (config.get "core:tools")
     let result =
         ExecProcess (fun info ->
             info.FileName <- config.get "core:tools" @@ nuget
             info.Arguments <- args) (TimeSpan.FromMinutes 5.)

     if result <> 0 then
         failwith "NUnit.Runners directory not found, and NuGet install failed."

let ensureSpecFlowRunner (config : Map<string, string>) =
  if not (directoryExists <| config.get "core:tools" @@ specFlowRunners) then
     let args =
         sprintf "install SpecFlow -ExcludeVersion -OutputDirectory \"%s\""
             (config.get "core:tools")
     let result =
         ExecProcess (fun info ->
             info.FileName <- config.get "core:tools" @@ nuget
             info.Arguments <- args) (TimeSpan.FromMinutes 5.)

     if result <> 0 then
         failwith "SpecFlow Runner directory not found, and NuGet install failed."