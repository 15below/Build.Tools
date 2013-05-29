#r "./fake/fakelib.dll"
#load "./Utils.fsx"

open Fake
open Utils
open System

let run (config : Map<string, string>) _ =
    let testDlls = !! (sprintf @".\**\bin\%s\**\*.Features.dll" (config.get "build:configuration"))
    if Seq.length testDlls > 0 then
        ensureNunitRunner config
        ensureSpecFlowRunner config
        testDlls
        |> NUnit 
            (fun defaults ->
                { defaults with 
                    ToolPath = config.get "core:tools" @@ nunitRunners
                    OutputFile = "SpecFlowResult.xml"
                 })
        SpecFlow
            (fun defaults ->
                { defaults with
                    SubCommand = ""
                    ProjectFile = !! @".\**\*.Features.csproj" |>Seq.head
                    XmlTestResultFile = "SpecFlowResult.xml"
                    OutputFile = "SpecFlowResult.html"
                 })

