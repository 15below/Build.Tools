#r    "./Fake/FakeLib.dll"

open Fake
open System
open System.IO
open Microsoft.FSharp.Collections

let nunitRunners = @"./NUnit.Runners/tools"
let specFlowRunners = "./SpecFlow/tools"
let nuget = @"./nuget/nuget.exe"

type Map<'Key,'Value when 'Key : comparison> with
    member this.get (name: 'Key) =
        this |> Map.find name

type PackageType =
    | NuGet
    | Paket

let packageType (config : Map<string, string>) =
    let rootDir =
        Directory.GetParent(config.get "core:tools").FullName
    match File.Exists(rootDir @@ "paket.dependencies") with
    | true -> Paket
    | false -> NuGet

let ensureNunitRunner (config : Map<string, string>) =
  if not (directoryExists <| config.get "core:tools" @@ nunitRunners) then
     let args =
         sprintf "install NUnit.Runners -ExcludeVersion -Version 2.6.4 -OutputDirectory \"%s\""
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
