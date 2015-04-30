#r    @"../../../packages/FAKE/tools/FakeLib.dll"
#load "./Utils.fsx"

open System
open System.IO
open Fake
open Utils

//let currentDir = Environment.CurrentDirectory + "/.." //for testing
let currentDir = Environment.CurrentDirectory 

let run script dir =
    tracefn "docker: running %s" script
    if File.Exists (dir @@ script) then
        sprintf "running script %s" (dir @@ script) |> trace
        let result = ExecProcess (fun psi ->
                        psi.FileName <- "/bin/bash" 
                        psi.Arguments <- dir @@ script
                        psi.WorkingDirectory <- dir) (TimeSpan.MaxValue)
        if result <> 0 then failwith (sprintf "failure executing %s" script)

let private execProcess fileName args wd =
    let result = ExecProcess (fun psi ->
                    psi.FileName <- fileName 
                    psi.Arguments <- args 
                    psi.WorkingDirectory <- wd) (TimeSpan.FromHours 1.0)
    if result <> 0 then 
        failwith (sprintf "ExecProcess failed (code: %i) for %s %s" result fileName args)
       
let private buildImage (config: Map<string, string>) name dir =
    run "pre.sh" dir
    tracefn "docker: running Dockerfile in: %s" dir
    let image = sprintf "%s/%s:%s" (config.get "docker:registry") name (config.get "versioning:build")
    execProcess "docker" (sprintf "build -t %s %s" image dir) dir
    tracefn "docker: tagging with latest: %s" dir
    let registry = (config.get "docker:registry")
    let latest = sprintf "%s/%s:latest" registry name
    execProcess "docker" (sprintf "tag %s/%s:%s %s" registry name (config.get "versioning:build") latest) dir
    if config.ContainsKey "docker:registry" && not isLocalBuild then
        tracefn "docker: pushing: %s" image
        execProcess "docker" (sprintf "push %s" image) dir
        tracefn "docker: pushing: %s" image
        execProcess "docker" (sprintf "push %s" latest) dir
    else 
        trace "docker: config key [docker:registry] not found. skipping docker push"
    run "post.sh" dir


let dockerize (config: Map<string, string>) _ =
    Directory.EnumerateDirectories (currentDir @@ "docker")
    |> Seq.map (fun d -> d, DirectoryInfo(d).Name)
    |> Seq.iter (fun (dir, name) ->
        buildImage config name dir)

(*
let config = Map ["versioning:build", "1235"; ]
let  _ = dockerize config ()
run "pre.sh" (currentDir @@ "docker/versionedstorage")
*)
