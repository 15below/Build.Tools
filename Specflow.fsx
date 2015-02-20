#r    @"../../../packages/FAKE/tools/fakelib.dll"
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
        testDlls
        |> NUnit 
            (fun defaults ->
                { defaults with 
                    Out = specFlowResultTextFile
                    OutputFile = specFlowResultXmlFile
                    IncludeCategory = testCategories 
                    ErrorLevel = DontFailBuild
                    DisableShadowCopy = false
                    ShowLabels = true
                    TimeOut = TimeSpan.FromHours 1.
                    })       
        generateSpecFlowReport config
