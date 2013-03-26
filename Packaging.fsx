#r "./fake/fakelib.dll"
#load "./Utils.fsx"

open System
open System.IO
open Fake

let private nuget = "./nuget/nuget.exe"

let private filterPackageable proj =
    Path.GetDirectoryName proj @@ Path.GetFileNameWithoutExtension proj + ".nuspec"
        |> (fun spec -> FileInfo spec)
        |> (fun file -> 
            match file.Exists with
                | true -> Some proj
                | _ -> None)

let private packageProject (config: Map<string, string>) proj =
    let args =
        sprintf "pack \"%s\" -OutputDirectory \"%s\" -Properties Configuration=%s" 
            proj
            (config.get "packaging:output")
            (config.get "build:configuration")

    let result =
        ExecProcess (fun info ->
            info.FileName <- config.get "core:tools" @@ nuget
            info.WorkingDirectory <- DirectoryName proj
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)
    
    if result <> 0 then failwithf "Error packaging NuGet package. Project file: %s" proj

let private restorePackages (config: Map<string, string>) file =
    RestorePackage (fun x ->
        { x with ToolPath = config.get "core:tools" @@ nuget }) file

let private updatePackages (config: Map<string, string>) file =
    let specificId = config.get "packaging:updateid"
    let args =
        sprintf "update \"%s\"%s"
            file
            (if isNotNullOrEmpty specificId then sprintf " -Id \"%s\"" specificId else "")
    let result =
        ExecProcess (fun info ->
            info.FileName <- config.get "core.tools" @@ nuget
            info.WorkingDirectory <- DirectoryName file
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)

    if result <> 0 then failwithf "Error updating NuGet package %s" specificId

let private pushPackages (config: Map<string, string>) nupkg =
    let pushurl = config.get "packaging:pushurl"
    let apikey = config.get "packaging:apikey"

    if isNullOrEmpty pushurl || isNullOrEmpty apikey then failwith "You must specify both apikey and pushurl to push NuGet packages."

    let args =
        sprintf "push \"%s\" %s -s \"%s\""
            nupkg
            apikey
            pushurl
    let result =
        ExecProcess (fun info ->
            info.FileName <- config.get "core.tools" @@ nuget
            info.WorkingDirectory <- DirectoryName nupkg
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)

    if result <> 0 then failwithf "Error pushing NuGet package %s" nupkg

let restore config _ =
    !! "./**/packages.config"
        |> Seq.iter (restorePackages config)

let update config _ =
    !! "./**/packages.config"
        |> Seq.iter (updatePackages config)

let package config _ =
    !! "./**/*.*proj"
        |> Seq.choose filterPackageable
        |> Seq.iter (packageProject config)

let push (config : Map<string, string>) _ =
    !! (config.get "packaging:output" @@ "./**/*.nupkg")
        |> Seq.iter (pushPackages config)