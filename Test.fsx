#r    @"../../../packages/FAKE/tools/FakeLib.dll"
#load "./Utils.fsx"

open Fake
open Fake.Testing
open Utils
open System
open System.IO

let run (config : Map<string, string>) _ =
    let testDlls = !! (sprintf @".\**\bin\%s\**\*.Tests.dll" (config.get "build:configuration"))
    let nunitRunner = !! @".\**\nunit*-console.exe" |> Seq.head 
        
    if Seq.length testDlls > 0 then
        match nunitRunner |> Path.GetFileName with
        | "nunit3-console.exe" ->
            testDlls
            |> NUnit3 (fun p -> { p with ToolPath = nunitRunner })
        | _ -> 
            testDlls
            |> NUnit (fun p -> { p with ToolPath = "packages/NUnit.Runners/tools" })
