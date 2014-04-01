#r "./fake/fakelib.dll"
#r "System.Xml.Linq.dll"
#r "System.IO.Compression.dll"
#load "./Utils.fsx"

open System
open System.IO
open System.Xml.Linq
open System.IO.Compression
open Fake
open Utils

let private nuget = @"NuGet\NuGet.exe"

let private filterPackageable proj =
    Path.GetDirectoryName proj @@ Path.GetFileNameWithoutExtension proj + ".nuspec"
        |> (fun spec -> FileInfo spec)
        |> (fun file -> 
            match file.Exists with
                | true -> Some proj
                | _ -> None)

let private packageProject (config: Map<string, string>) outputDir proj =

    let args =
        sprintf "pack \"%s\" -OutputDirectory \"%s\" -IncludeReferencedProjects -Properties Configuration=%s;VisualStudioVersion=%s" 
            proj
            outputDir
            (config.get "build:configuration")
            (config.get "vs:version")

    let result =
        ExecProcess (fun info ->
            info.FileName <- config.get "core:tools" @@ nuget
            info.WorkingDirectory <- DirectoryName proj
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)
    
    if result <> 0 then failwithf "Error packaging NuGet package. Project file: %s" proj

let private packageDeployment (config: Map<string, string>) outputDir proj =

    let args =
        sprintf "pack \"%s\" -OutputDirectory \"%s\" -Properties Configuration=%s;VisualStudioVersion=%s" 
            proj
            outputDir
            (config.get "build:configuration")
            (config.get "vs:version")

    let result =
        ExecProcess (fun info ->
            info.FileName <- config.get "core:tools" @@ nuget
            info.WorkingDirectory <- DirectoryName proj
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)
    
    if result <> 0 then failwithf "Error packaging NuGet package. Project file: %s" proj

let private installPackageOptions (config: Map<string, string>) =
    let packagePath = (config.get "packaging:packages")
    if isNullOrEmpty packagePath then
        ""
    else
       sprintf @"-OutputDirectory ""%s""" packagePath 

let private restorePackages (config: Map<string, string>) file =
    let timeOut = TimeSpan.FromMinutes 5.
    let args = sprintf @"install ""%s"" %s" file (installPackageOptions config)
    let result = ExecProcess (fun info ->
                        info.FileName <- config.get "core:tools" @@ nuget
                        info.WorkingDirectory <- Path.GetFullPath(".")
                        info.Arguments <- args) timeOut
    if result <> 0 then failwithf "Error during Nuget update. %s %s" (config.get "core:tools" @@ nuget) args

let private updatePackages (config: Map<string, string>) file =
    let specificId = config.get "packaging:updateid"
    if 
        isNullOrEmpty specificId ||
        (isNotNullOrEmpty specificId && File.ReadAllText(file).Contains(specificId)) then
            let args =
                sprintf "update \"%s\"%s"
                    file
                    (if isNotNullOrEmpty specificId then sprintf " -Id \"%s\"" specificId else "")
            let result =
                ExecProcess (fun info ->
                    info.FileName <- config.get "core:tools" @@ nuget
                    info.WorkingDirectory <- DirectoryName file
                    info.Arguments <- args) (TimeSpan.FromMinutes 5.)

            if result <> 0 then failwithf "Error updating NuGet package %s" specificId

let private pushPackages (config: Map<string, string>) pushurl apikey nupkg =

    if isNullOrEmpty pushurl || isNullOrEmpty apikey then failwith "You must specify both apikey and pushurl to push NuGet packages."

    let args =
        sprintf "push \"%s\" %s -s \"%s\""
            nupkg
            apikey
            pushurl
    let result =
        ExecProcess (fun info ->
            info.FileName <- config.get "core:tools" @@ nuget
            info.WorkingDirectory <- DirectoryName nupkg
            info.Arguments <- args) (TimeSpan.FromMinutes 5.)

    if result <> 0 then failwithf "Error pushing NuGet package %s" nupkg

let restore config _ =
    !! "./**/packages.config"
        |> Seq.iter (restorePackages config)

let update config _ =
    !! "./**/packages.config"
        |> Seq.iter (updatePackages config)

let package (config : Map<string, string>) _ =
    CleanDir (config.get "packaging:output")

    !! "./**/*.*proj"
        |> Seq.choose filterPackageable
        |> Seq.iter (packageProject config (config.get "packaging:output"))

let packageDeploy (config : Map<string, string>) _ =
    CleanDir (config.get "packaging:deployoutput")

    !! "./**/Deploy/*.nuspec"
        |> Seq.iter (packageDeployment config (config.get "packaging:deployoutput"))

let push (config : Map<string, string>) _ =
    let pushurl = config.get "packaging:pushurl"
    let apikey = config.get "packaging:apikey"
    !! (config.get "packaging:output" @@ "./**/*.nupkg")
        |> Seq.iter (pushPackages config pushurl apikey)

let pushDeploy (config : Map<string, string>) _ =
    let pushurl = config.get "packaging:deploypushurl"
    let apikey = config.get "packaging:deployapikey"
    !! (config.get "packaging:deployoutput" @@ "./**/*.nupkg")
        |> Seq.iter (pushPackages config pushurl apikey)

let private makeConstraint vs =
    match Version.TryParse vs with
    | true, version -> 
        Some (sprintf "[%O,%i)" version (version.Major + 1))
    | _ -> None

let private applyConstraint (xml:string) =
    let xn s = XName.Get(s)    
    let xns s = XNamespace.Get(s)
    let doc = XDocument.Parse(xml)
    let ns = xns "http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd"
    doc
        .Element(ns + ("package"))
        .Element(ns + "metadata")
        .Element(ns + "dependencies")
        .Elements(ns + "dependency")
        |> Seq.map (fun e ->  e, makeConstraint (e.Attribute(xn "version").Value))
        |> Seq.choose (fun (e, v) -> if v.IsSome then Some (e, v.Value) else None)
        |> Seq.iter (fun (e, v) -> e.SetAttributeValue(xn "version", v))
    doc.ToString()

let private transform nuSpec f =
    use xp = new ZipArchive(new FileStream(nuSpec, FileMode.Open), ZipArchiveMode.Update)
    let entry =
        xp.Entries
        |> Seq.find (fun x -> x.Name.EndsWith (".nuspec"))
    let (text:string) =
        use sr = new StreamReader(entry.Open())
        sr.ReadToEnd() |> f
    use w = new StreamWriter(entry.Open())
    w.Write text
    w.BaseStream.SetLength(w.BaseStream.Position)
        
//constrains a nuget package to sensible dependency ranges
let constrain (config : Map<string, string>) () =
    !! (config.get "packaging:output" @@ "./**/*.nupkg")
    |> Seq.iter (fun f -> transform f applyConstraint)
