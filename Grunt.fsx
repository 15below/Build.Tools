#r "./fake/fakelib.dll"
#load "./Utils.fsx"

open Fake
open Utils
open System

let npm = System.IO.Path.Combine("C:\\Program Files (x86)\\", "nodejs\\npm.cmd")
let grunt = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm\\grunt.cmd")

let install (config : Map<string, string>) _ =

    let args = "install"

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