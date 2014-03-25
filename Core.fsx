#r    "./fake/fakelib.dll"
#load "./Utils.fsx"
#load "./Packaging.fsx"
#load "./Versioning.fsx"
#load "./Solution.fsx"
#load "./Test.fsx"
#load "./Specflow.fsx"

open System.IO
open Fake

let config = 
    Map.ofList [
        "build:configuration", environVarOrDefault "configuration"         "Release"
        "build:solution",      environVar          "solution"
        "core:tools",          environVar          "tools"
        "packaging:output",    environVarOrDefault "output"                (sprintf "%s\output" (Path.GetFullPath(".")))
        "packaging:updateid",  environVarOrDefault "updateid"              ""
        "packaging:pushurl",   environVarOrDefault "pushurl"               ""
        "packaging:apikey",    environVarOrDefault "apikey"                ""
        "packaging:packages",  environVarOrDefault "packages"              ""
        "versioning:build",    environVarOrDefault "build_number"          "0"
        "versioning:branch",   match environVar "teamcity_build_branch" with
                                   | "<default>" -> environVar "vcsroot_branch"
                                   | _ -> environVar "teamcity_build_branch"
        "vs:version"           environVarOrDefault "vs_version"            "11.0" ]

// Target definitions
Target "Default"           <| DoNothing
Target "Packaging:Package" <| Packaging.package config
Target "Packaging:Restore" <| Packaging.restore config
Target "Packaging:Update"  <| Packaging.update config
Target "Packaging:Push"    <| Packaging.push config
Target "Solution:Build"    <| Solution.build config
Target "Solution:Clean"    <| Solution.clean config
Target "Versioning:Update" <| Versioning.update config
Target "Test:Run"          <| Test.run config
Target "SpecFlow:Run"      <| Specflow.run config

// Build order
"Solution:Clean"
    ==> "Packaging:Restore"
    ==> "Versioning:Update"
    ==> "Solution:Build"
    ==> "Packaging:Package"
    ==> "SpecFlow:Run"
    ==> "Test:Run"
    =?> ("Packaging:Push", not isLocalBuild)
    ==> "Default"

RunParameterTargetOrDefault "target" "Default"
