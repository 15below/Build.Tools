#r "./fake/fakelib.dll"
#load "./Utils.fsx"

open Fake
open Utils
open System

let private specFlowResultTextFile = "SpecFlowResult.txt"
let private specFlowResultHtmlFile = "SpecFlowResult.html"
let private specFlowResultXmlFile = "SpecFlowResult.xml"
let private testCategories = environVarOrDefault "TestCategories" ""

let private generateSpecFlowReport (config : Map<string, string>) =   
    SpecFlow
        (fun defaults ->
            { defaults with
                ToolPath = config.get "core:tools" @@ specFlowRunners
                SubCommand = "nunitexecutionreport"
                ProjectFile = !! @".\**\*.Features.csproj" |>Seq.head
                XmlTestResultFile = specFlowResultXmlFile
                TestOutputFile = specFlowResultTextFile
                OutputFile = specFlowResultHtmlFile
             })

let run (config : Map<string, string>) _ =
    
    DeleteFile specFlowResultTextFile
    DeleteFile specFlowResultHtmlFile
    DeleteFile specFlowResultXmlFile
    
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
                        Out = specFlowResultTextFile
                        OutputFile = specFlowResultXmlFile
                        IncludeCategory = testCategories 
                        ErrorLevel = DontFailBuild
                        DisableShadowCopy = true
                        ShowLabels = true
                        TimeOut = TimeSpan(1,0,0)
                     })
        with
        | e ->
            generateSpecFlowReport config
            raise e
        generateSpecFlowReport config