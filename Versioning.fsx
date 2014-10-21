#r "./fake/fakelib.dll"
#load "./Utils.fsx"

open System
open System.Text.RegularExpressions
open System.Xml
open Fake
open Fake.Git
open Utils

let private readAssemblyVersion file =
    ReadFile file
        |> Seq.find (fun line -> not (line.StartsWith("//") || line.StartsWith("'")) && line.Contains "AssemblyVersion")
        |> (fun line -> Regex.Match(line, @"(?<=\().+?(?=\))").Value)
        |> (fun version -> Version (version.Trim [|'"'|]))

let private escapeBranchName rawName =
    let regex = System.Text.RegularExpressions.Regex(@"[^0-9A-Za-z-]")
    let safeChars = regex.Replace(rawName, "-")
    safeChars.[0..(min 9 <| String.length safeChars - 1)]

    
let private constructInfoVersion (config: Map<string, string>) (fileVersion: Version) file branchName =
    let infoVersion = 
        Version (
            fileVersion.Major, 
            fileVersion.Minor, 
            fileVersion.Build)

    let suffix =
        match isLocalBuild with
            | true -> 
                "-" + (branchName |> escapeBranchName) + "-local"
            | _ ->
                match config.get "versioning:branch" with
                    | "master" -> 
                        "." + config.get "versioning:build"
                    | _ -> 
                        "-" + (config.get "versioning:branch" |> escapeBranchName) + "-" + config.get "versioning:build" + "-ci"

    infoVersion.ToString() + suffix


let private constructVersions (config: Map<string, string>) file branchName =
    let fileVersion = readAssemblyVersion file

    let assemblyVersion = 
        Version (
            fileVersion.Major, 
            fileVersion.Minor,
            fileVersion.Build, 
            int <| config.get "versioning:build")

    assemblyVersion.ToString(), (constructInfoVersion config fileVersion file branchName)

let private updateAssemblyInfo config branchName file =
    let versions = constructVersions config file branchName

    ReplaceAssemblyInfoVersions (fun x ->
        {
            x with
                 OutputFileName = file
                 AssemblyConfiguration = config.get "build:configuration"
                 AssemblyVersion = fst versions
                 AssemblyFileVersion = fst versions
                 AssemblyInformationalVersion = snd versions
        })

let private updateDeployNuspec config (file:string) =
    let xdoc = new XmlDocument()
    ReadFileAsString file |> xdoc.LoadXml
    
    let versionNode = xdoc.SelectSingleNode "/package/metadata/version"

    let semVer = SemVerHelper.parse(versionNode.InnerText.ToString())

    let fileVersion = new Version(semVer.Major, semVer.Minor, semVer.Patch, 0)

    versionNode.InnerText <- (constructInfoVersion config fileVersion file branchName)
    
    WriteStringToFile false file (xdoc.OuterXml.ToString().Replace("><",">\n<"))

let update config _ =
    let branchName = getBranchName "."
    !! "./**/AssemblyInfo.cs"
    ++ "./**/AssemblyInfo.vb"
    ++ "./**/AssemblyInfo.fs"
    ++ "./**/AssemblyInfo.vb"
        |> Scan
        |> Seq.iter (updateAssemblyInfo config branchName)

let updateDeploy config _ =
    !! "./**/Deploy/*.nuspec"
        |> Seq.iter (updateDeployNuspec config)
