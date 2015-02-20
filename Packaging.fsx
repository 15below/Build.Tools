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

let private paket = ".paket" @@ "paket.exe" 

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

let restore config _ =
    let curDir = System.Environment.CurrentDirectory
    let result =
        ExecProcess (fun info ->
                info.FileName <- curDir @@ paket
                info.WorkingDirectory <- DirectoryName "."
                info.Arguments <- "restore") (TimeSpan.FromMinutes 5.)
    if result <> 0 then failwith "Paket restore failed."
        

let cleanDirOnceHistory = new System.Collections.Generic.List<string>()
let CleanDirOnce dir =
    if (cleanDirOnceHistory.Contains(dir)) = false then
        cleanDirOnceHistory.Add(dir)
        CleanDir dir


/// Paket parameter type
type PaketPackParams = 
    { ToolPath : string
      TimeOut : TimeSpan
      Version : string
      Authors : string list
      Project : string
      Title : string
      Summary : string
      Description : string
      Tags : string
      ReleaseNotes : string
      Copyright : string
      OutputPath : string }

/// Paket pack default parameters  
let PaketPackDefaults() : PaketPackParams = 
    { ToolPath = findToolFolderInSubPath "paket.exe" (currentDirectory @@ ".paket" @@ "paket.exe")
      TimeOut = TimeSpan.FromMinutes 5.
      Version = 
          if not isLocalBuild then buildVersion
          else "0.1.0.0"
      Authors = []
      Project = ""
      Title = ""
      Summary = null
      Description = null
      Tags = null
      ReleaseNotes = null
      Copyright = null
      OutputPath = "./temp" }

/// Creates a new NuGet package by using Paket pack on all paket.template files in the given root directory.
/// ## Parameters
/// 
///  - `setParams` - Function used to manipulate the default parameters.
///  - `rootDir` - The paket.template files.
let PaketPack setParams rootDir = 
    traceStartTask "PaketPack" rootDir
    let parameters : PaketPackParams = PaketPackDefaults() |> setParams

    let packResult =
        ExecProcess (fun info ->
            info.FileName <- parameters.ToolPath @@ "paket.exe"
            info.Arguments <- sprintf "pack output %s" parameters.OutputPath) parameters.TimeOut

    if packResult <> 0 then failwithf "Error during packing %s." rootDir

    traceEndTask "PaketPack" rootDir


/// Paket parameter type
type PaketPushParams = 
    { ToolPath : string
      TimeOut : TimeSpan
      PublishUrl : string
      AccessKey : string }

/// Paket push default parameters
let PaketPushDefaults() : PaketPushParams = 
    { ToolPath = findToolFolderInSubPath "paket.exe" (currentDirectory @@ ".paket" @@ "paket.exe")
      TimeOut = TimeSpan.FromMinutes 5.
      PublishUrl = "https://nuget.org"
      AccessKey = null }

/// Pushes a NuGet package to the server by using Paket push.
/// ## Parameters
/// 
///  - `setParams` - Function used to manipulate the default parameters.
///  - `packages` - The .nupkg files.
let PaketPush setParams packages =
    let packages = Seq.toList packages
    traceStartTask "PaketPush" (separated ", " packages)
    let parameters : PaketPushParams = PaketPushDefaults() |> setParams

    for package in packages do
        let pushResult =
            ExecProcess (fun info ->
                info.FileName <- parameters.ToolPath @@ "paket.exe"
                info.Arguments <- sprintf "push url %s file %s" parameters.PublishUrl package) System.TimeSpan.MaxValue
        if pushResult <> 0 then failwithf "Error during pushing %s." package

    traceEndTask "PaketPush" (separated ", " packages)

let package config _ =
    PaketPack (fun p -> { p  with OutputPath = Map.find "packaging:output" config }) "."

let push config _ =
    trace "do we get here?"
    setEnvironVar "NugetApiKey" (Map.find "packaging:apikey" config)
    !! ((Map.find "packaging:output" config) @@ "*.nupkg")
    |> PaketPush (fun p -> { p with PublishUrl = Map.find "packaging:pushurl" config })