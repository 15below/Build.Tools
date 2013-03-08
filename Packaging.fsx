#r "./fake/fakelib.dll"

open System
open System.IO
open Fake

let private nuget = "./nuget/nuget.exe"
let private configs = "./**/packages.config"
let private projects = "./**/*.*proj"

let private filterPackageable proj =
    Path.GetDirectoryName proj @@ Path.GetFileNameWithoutExtension proj + ".nuspec"
        |> (fun spec -> FileInfo spec)
        |> (fun file -> 
            match file.Exists with
                | true -> Some proj
                | _ -> None)

let private packageProject config proj =
    let args =
        sprintf "pack \"%s\" -OutputDirectory \"%s\" -Properties Configuration=%s" 
            proj
            (config |> Map.find "output")
            (config |> Map.find "configuration")

    let result =
        ExecProcess (fun info ->
            info.FileName <- (config |> Map.find "tools") @@ nuget
            info.WorkingDirectory <- DirectoryName proj
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)
    
    if result <> 0 then failwithf "Error packaging NuGet package. Project file: %s" proj

let private restorePackages config file =
    RestorePackage (fun x ->
        { x with ToolPath = (config |> Map.find "tools") @@ nuget }) file

let restore config _ =
    !! configs
        |> Seq.iter (restorePackages config)

let package config _ =
    !! projects
        |> Seq.choose filterPackageable
        |> Seq.iter (packageProject config)