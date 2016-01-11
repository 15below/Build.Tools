#r    @"../../../packages/FAKE/tools/FakeLib.dll"
#load "./Utils.fsx"
#load "./Npm.fsx"

open Fake
open Utils
open Npm
open System
open System.IO

let grunt = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm\\node_modules\\grunt-cli\\bin\\grunt")

let install = Npm.install

let run (config : Map<string, string>) _ =

    let env = match config.TryFind "grunt:environment" with
              | Some x -> x
              | _ -> "dev"

    let verbose =
        match config.TryFind "grunt:verbose" with
        | Some x ->
              match x with
              |"true" -> "--verbose"
              | _ -> ""
        | _ -> ""

    let args = "\"" + grunt + "\" --env=\"" + env + "\" " + verbose

    let result =
        ExecProcess (fun info ->
            info.FileName <- Npm.node
            info.WorkingDirectory <- ".\\build"
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)

    if result <> 0 then failwith (sprintf "Grunt has exited with a non-zero error level: %d" result)

    ()

let karma (config : Map<string, string>) _ =

    let args = "\"" + grunt + "\" karma"

    let result =
        ExecProcess (fun info ->
            info.FileName <- Npm.node
            info.WorkingDirectory <- ".\\build"
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)

    if result <> 0 then failwith (sprintf "Karma reporting failure - at least one test has failed: %d" result)

    ()

let protractor (config : Map<string, string>) _ =

    let args = "\"" + grunt + "\" protractor"

    let result =
        ExecProcess (fun info ->
            info.FileName <- Npm.node
            info.WorkingDirectory <- ".\\build"
            info.Arguments <- args) (TimeSpan.FromMinutes 20.)

    if result <> 0 then failwith (sprintf "Protractor reporting failure - at least one test has failed: %d" result)
    ()
