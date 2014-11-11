#r "./fake/fakelib.dll"
#load "./Utils.fsx"

open System
open System.IO
open Fake
open Utils

//let currentDir = Environment.CurrentDirectory + "/.." //for testing
let currentDir = Environment.CurrentDirectory 

let run script dir =
    if File.Exists (dir @@ script) then
        sprintf "running script %s" (dir @@ script) |> trace
        ExecProcess (fun psi ->
                        psi.FileName <- "/bin/bash" 
                        psi.Arguments <- dir @@ script
                        psi.WorkingDirectory <- dir) (TimeSpan.MaxValue)
    else 0

let private updateDockerFile dir =
    trace (sprintf "updating docker file in directory: %s" dir)
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
    let result = ExecProcess (fun psi ->
                    psi.FileName <- "docker"
                    psi.Arguments <- sprintf "build -t %s:%s %s" name (config.get "versioning:build") dir
                    psi.WorkingDirectory <- dir) (TimeSpan.FromHours 1.0)
    let post = run "post.sh" dir
    result + pre + post


let dockerize (config: Map<string, string>) _ =
    Directory.EnumerateDirectories (currentDir @@ "docker")
    |> Seq.map (fun d -> d, DirectoryInfo(d).Name)
    |> Seq.map (fun (dir, name) ->
        name, buildImage config name dir)
    |> Seq.filter (fun (_,res) -> res > 0)
    |> Seq.toList
    |> function
       | [] -> ()
       | failed -> failwith "some docker images failed to generate %A" failed

(*
let config = Map ["versioning:build", "1235"; ]
let  _ = dockerize config ()
run "pre.sh" (currentDir @@ "docker/versionedstorage")
*)
