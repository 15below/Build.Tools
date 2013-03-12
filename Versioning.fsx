#r "./fake/fakelib.dll"
#load "./Utils.fsx"

open System
open System.Text.RegularExpressions
open Fake
open Fake.Git

let private readAssemblyVersion file =
    ReadFile file
        |> Seq.find (fun line -> line.Contains "AssemblyVersion")
        |> (fun line -> Regex.Match(line, @"(?<=\().+?(?=\))").Value)
        |> (fun version -> Version (version.Trim [|'"'|]))

let private constructVersions (config: Map<string, string>) file =
    let fileVersion = readAssemblyVersion file

    let assemblyVersion = 
        Version (
            fileVersion.Major, 
            fileVersion.Minor,
            fileVersion.Build, 
            int <| config.get "versioning:build")

    let infoVersion = 
        Version (
            fileVersion.Major, 
            fileVersion.Minor, 
            fileVersion.Build)

    let suffix =
        match isLocalBuild with
            | true -> 
                "-" + getBranchName (DirectoryName file) + "-local"
            | _ ->
                match config.get "versioning:branch" with
                    | "master" -> 
                        "." + config.get "versioning:build"
                    | _ -> 
                        "-" + config.get "versioning:branch" + "-" + config.get "versioning:build" + "-ci"

    assemblyVersion.ToString(), infoVersion.ToString() + suffix

let private updateAssemblyInfo config file =
    let versions = constructVersions config file

    ReplaceAssemblyInfoVersions (fun x ->
        {
            x with
                 OutputFileName = file
                 AssemblyConfiguration = config.get "build:configuration"
                 AssemblyVersion = fst versions
                 AssemblyFileVersion = fst versions
                 AssemblyInformationalVersion = snd versions
        })

let update config _ =
    !+ "./**/AssemblyInfo.cs"
    ++ "./**/AssemblyInfo.fs"
        |> Scan
        |> Seq.iter (updateAssemblyInfo config)
