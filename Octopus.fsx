#r "./fake/fakelib.dll"

open Fake
open Fake.OctoTools

let run config environment _ =
  let projectName = config |> Map.tryFind "octopus:projectname"
  let server = config |> Map.tryFind "octopus:server"
  let apiKey = config |> Map.tryFind "octopus:apikey"
  let deploy = config |> Map.tryFind "octopus:deploy"
  let octoTools = (config |> Map.find "core:tools") @@ "Octo"

  match projectName, server, apiKey, deploy with
    | Some projectName, Some server, Some apikey, Some "true" ->
      let deployTo = environment

      let release = { releaseOptions with Project = projectName }
      let deploy = { deployOptions with DeployTo = deployTo }

      Octo (fun octoParams ->
            { octoParams with
                ToolPath = octoTools
                Server = { Server = server; ApiKey = apikey }
                Command = CreateRelease (release, Some deploy) })
    | _, _, _, Some "false" | _, _, _, None -> ()
    | _ -> failwith "Octopus deploy not running - project name, server address or api key missing"
