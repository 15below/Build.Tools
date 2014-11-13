#r "./fake/fakelib.dll"
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
        ExecProcess (fun psi ->
                        psi.FileName <- "/bin/bash" 
                        psi.Arguments <- dir @@ script
                        psi.WorkingDirectory <- dir) (TimeSpan.MaxValue)
    else 0

let private updateDockerFile dir =
    tracefn "docker: updating docker file in directory: %s" dir
    let dockerFile = dir @@ "Dockerfile"
    let dockerFileOrig = dir @@ "Dockerfile.orig"
    File.Copy(dockerFile, dockerFileOrig)
    let file = File.ReadAllText (dir @@ "Dockerfile")
    file.Replace(@"[[HACK]]", string DateTime.Now.Ticks)
    |> fun s -> File.WriteAllText(dir @@ "Dockerfile", s)
    File.Delete(dockerFile)
    File.Move(dockerFileOrig, dockerFile)

let private buildImage (config: Map<string, string>) name dir =
    updateDockerFile dir
    let pre = run "pre.sh" dir
    tracefn "docker: running Dockerfile in: %s" dir
    let image = sprintf "%s/%s:%s" (config.get "docker:registry") name (config.get "versioning:build")
    let result = ExecProcess (fun psi ->
                    psi.FileName <- "docker"
                    psi.Arguments <- sprintf "build -t %s %s" image dir
                    psi.WorkingDirectory <- dir) (TimeSpan.FromHours 1.0)
    tracefn "docker: tagging with latest: %s" dir
    let tag = ExecProcess (fun psi ->
                    psi.FileName <- "docker"
                    psi.Arguments <- sprintf "tag %s/%s:%s %s:latest" (config.get "docker:registry") name (config.get "versioning:build") name
                    psi.WorkingDirectory <- dir) (TimeSpan.FromHours 1.0)
    tracefn "docker: pushing: %s" image
    let push = ExecProcess (fun psi ->
                    psi.FileName <- "docker"
                    psi.Arguments <- sprintf "push %s" image
                    psi.WorkingDirectory <- dir) (TimeSpan.FromHours 1.0)
    let post = run "post.sh" dir
    result + tag + pre + post + push, image


let dockerize (config: Map<string, string>) _ =
    Directory.EnumerateDirectories (currentDir @@ "docker")
    |> Seq.map (fun d -> d, DirectoryInfo(d).Name)
    |> Seq.map (fun (dir, name) ->
        name, buildImage config name dir)
    |> Seq.filter (fun (_,(res, image)) -> res > 0)
    |> Seq.toList
    |> function
       | [] -> ()
       | failed -> failwith "some docker images failed to generate %A" failed

(*
let config = Map ["versioning:build", "1235"; ]
let  _ = dockerize config ()
run "pre.sh" (currentDir @@ "docker/versionedstorage")
*)
