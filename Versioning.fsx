#r "./fake/fakelib.dll"

open System
open System.Text.RegularExpressions
open Fake

let private readAssemblyVersion file =
    ReadFile file
        |> Seq.find (fun line -> line.Contains "AssemblyVersion")
        |> (fun line -> Regex.Match(line, @"(?<=\().+?(?=\))").Value)
        |> (fun version -> version.Trim [|'"'|])

let private updateAssemblyInfo config file =
    let assemblyVersion = readAssemblyVersion file

    ReplaceAssemblyInfoVersions (fun x ->
        {
            x with
                 OutputFileName = file
                 AssemblyConfiguration = (config |> Map.find "build:configuration")
                 AssemblyVersion = assemblyVersion
                 AssemblyFileVersion = assemblyVersion
                 AssemblyInformationalVersion = assemblyVersion + "-develop.local"
        })

let update config _ =
    !+ "./**/AssemblyInfo.cs"
    ++ "./**/AssemblyInfo.fs"
        |> Scan
        |> Seq.iter (updateAssemblyInfo config)