#r "./fake/fakelib.dll"
#load "./Utils.fsx"

open Fake
open Utils
open System
open System.IO

let npm64 = Path.Combine("C:\\Program Files\\", "nodejs\\npm.cmd")
let npm86 = Path.Combine("C:\\Program Files (x86)\\", "nodejs\\npm.cmd")
let grunt = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm\\grunt.cmd")

let install (config : Map<string, string>) _ =

    let args = "install"

    let npm = match File.Exists npm64 with | true -> npm64 | _ -> npm86

    let result =
        ExecProcess (fun info ->
            info.FileName <- npm
            info.WorkingDirectory <- ".\\build"
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)

    ()

let concatAndUglify (config : Map<string, string>) _ =

    let args = "--env=\"production\""

    let result =
        ExecProcess (fun info ->
            info.FileName <- grunt
            info.WorkingDirectory <- ".\\build"
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)

    ()

let karma (config : Map<string, string>) _ =

    let args = "karma"

    let result =
        ExecProcess (fun info ->
            info.FileName <- grunt
            info.WorkingDirectory <- ".\\build"
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)

    ()