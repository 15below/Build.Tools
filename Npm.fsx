#r    @"../../../packages/FAKE/tools/FakeLib.dll"
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

let install (config : Map<string, string>) _ =

    let args = "install"

    let workingdir =
        match config.TryFind "npm:dir" with
        | Some x when String.IsNullOrEmpty x = false -> x
        | _ -> ".\\build"

    let result =
        ExecProcess (fun info ->
            info.FileName <- npm
            info.WorkingDirectory <- workingdir
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)

    ()
