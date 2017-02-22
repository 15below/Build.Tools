#r    @"../../../packages/FAKE/tools/FakeLib.dll"
#load "./Utils.fsx"
#load "./Npm.fsx"

open Fake
open Utils
open System
open System.IO

let gulp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm\\node_modules\\gulp\\bin\\gulp.js")

let run (config : Map<string, string>) _ =

    let env =
        match config |> Map.tryFind "gulp:env" with
        | Some x -> x
        | _ -> "dev"

    let build =
        match config |> Map.tryFind "gulp:build" with
        | Some x -> x
        | _ -> "0.0.0"

    let args = sprintf "\"%s\" --env=%s --build=%s" gulp env build

    let result =
        ExecProcess (fun info ->
            info.FileName <- Npm.node
            info.WorkingDirectory <- ".\\build"
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)

    if result <> 0 then failwith (sprintf "Gulp has exited with a non-zero error code: %d" result)

    ()

let target (config : Map<string, string>) name _ =
    let args = "\"" + gulp + "\" " + name

    let result =
        ExecProcess(fun info ->
            info.FileName <- Npm.node
            info.WorkingDirectory <- ".\\build"
            info.Arguments <- args) (TimeSpan.FromMinutes 15.)

    if result <> 0 then failwith (sprintf "Gulp has exited with a non-zero error code: %d" result)

    ()
