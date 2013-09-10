#r "./fake/fakelib.dll"
#load "./Utils.fsx"

open Fake
open Utils
open System

let private generateSpecFlowReport (config : Map<string, string>) =
    SpecFlow
        (fun defaults ->
            { defaults with
                ToolPath = config.get "core:tools" @@ specFlowRunners
                SubCommand = "nunitexecutionreport"
                ProjectFile = !! @".\**\*.Features.csproj" |>Seq.head
                XmlTestResultFile = "SpecFlowResult.xml"
                TestOutputFile = "SpecFlowResult.txt"
                OutputFile = "SpecFlowResult.html"
             })

let run (config : Map<string, string>) _ =
    let testDlls = !! (sprintf @".\**\bin\%s\**\*.Features.dll" (config.get "build:configuration"))
    if Seq.length testDlls > 0 then
        ensureNunitRunner config
        ensureSpecFlowRunner config
        try
            testDlls
            |> NUnit 
                (fun defaults ->
                    { defaults with 
                        ToolPath = config.get "core:tools" @@ nunitRunners
                        Out = "SpecFlowResult.txt"
                        OutputFile = "SpecFlowResult.xml"
                     })
        with
        | e ->
            generateSpecFlowReport config
            raise e
        generateSpecFlowReport config