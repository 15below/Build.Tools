#r    @"../../../packages/FAKE/tools/fakelib.dll"
#r "System.Xml.Linq.dll"
#r "System.IO.Compression.dll"
#load "./Utils.fsx"

open System
open System.IO
open System.IO.Compression
open System.Text.RegularExpressions
open System.Xml.Linq
open Fake
open Utils

let private getPackageName nupkg =
    // Regex turns D:\output\My.Package.1.0.0.0.nupkg into My.Package
    let regex = new Regex(".*\\\\([^\\\\]+)\\.[\\d]+\\.[\\d]+\\.[\\d]+[\\.\\-][\\d\\-a-zA-Z]+\\.nupkg")
    regex.Replace(nupkg, "$1")

let private pushPackagesToDir (config: Map<string, string>) dir nupkg =
    let info = new FileInfo(nupkg)
    let name = getPackageName nupkg
    let directory = sprintf "%s\%s" dir name
    if (not (Directory.Exists(directory))) then 
        Directory.CreateDirectory(directory) |> ignore
    let file = info.CopyTo(sprintf "%s\%s" directory info.Name, true)
    sprintf "Pushed File: %s to: %s" info.Name directory |> ignore

let cleanDirOnceHistory = new System.Collections.Generic.List<string>()
let CleanDirOnce dir =
    if (cleanDirOnceHistory.Contains(dir)) = false then
        cleanDirOnceHistory.Add(dir)
        CleanDir dir

let package config _ =
    CleanDirOnce (Map.find "packaging:output" config)
    Paket.Pack (fun p -> { p  with OutputPath = Map.find "packaging:output" config })

let push config _ =
    setEnvironVar "nugetkey" (Map.find "packaging:apikey" config)
    Paket.Push (fun p -> { p with
                                PublishUrl = Map.find "packaging:pushurl" config
                                EndPoint   = Map.find "packaging:pushendpoint" config
                                WorkingDir = Map.find "packaging:output" config })
