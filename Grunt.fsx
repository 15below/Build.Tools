#r "./fake/fakelib.dll"
#load "./Utils.fsx"

open Fake
open Utils
open System
open System.IO

let nodeDir64 = "C:\\Program Files\\nodejs\\"
let nodeDir86 = "C:\\Program Files (x86)\\nodejs\\"
let nodeDir = match Directory.Exists nodeDir64 with | true -> nodeDir64 | _ -> nodeDir86
let node = Path.Combine(nodeDir, "node.exe")
let npm = Path.Combine(nodeDir, "npm.cmd")
let grunt = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm\\node_modules\\grunt-cli\\bin\\grunt")

let install (config : Map<string, string>) _ =

    let args = "install"

    let result =
        ExecProcess (fun info ->
            info.FileName <- npm
            info.WorkingDirectory <- ".\\build"
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)

    ()

let concatAndUglify (config : Map<string, string>) _ =

    let args = "\"" + grunt + "\" --env=\"production\""

    let result =
        ExecProcess (fun info ->
            info.FileName <- node
            info.WorkingDirectory <- ".\\build"
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)

    ()

let karma (config : Map<string, string>) _ =

    let args = "\"" + grunt + "\" karma"

    let result =
        ExecProcess (fun info ->
            info.FileName <- node
            info.WorkingDirectory <- ".\\build"
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)

    if result <> 0 then failwith "Karma reporting failure - at least one test has failed."