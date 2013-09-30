#r "./fake/fakelib.dll"
#load "./Utils.fsx"

open System
open Fake
open Utils

let private runCompiler target (config: Map<string, string>) =
    let setParams defaults =
        { defaults with
            Verbosity = Some(Quiet)
            Targets = [target]
            Properties =
                [
                    "Optimize", "True"
                    "DebugSymbols", "True"
                    "Configuration", config.get "build:configuration"
                ]
            MaxCpuCount = Some <| Some Environment.ProcessorCount }
    build setParams (config.get "build:solution")


let build (config: Map<string, string>) _ =
    runCompiler "Build" config

let clean (config: Map<string, string>) _ =
    runCompiler "Clean" config
