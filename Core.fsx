#r    "./fake/fakelib.dll"
#load "./Utils.fsx"
#load "./Packaging.fsx"
#load "./Versioning.fsx"
#load "./Solution.fsx"

open Fake

let config = 
    Map.ofList [
        "build:configuration", environVarOrDefault "configuration"         "Release"
        "build:solution",      environVar          "solution"
        "core:tools",          environVar          "tools"
        "packaging:output",    environVarOrDefault "output"                "C:/Packages"
        "versioning:build",    environVarOrDefault "build_number"          "0"
        "versioning:branch",   match environVar "teamcity_build_branch" with
                                   | "<default>" -> environVar "vcsroot_branch"
                                   | _ -> environVar "teamcity_build_branch"
    ]

Target "Default"           <| DoNothing
Target "Packaging:Package" <| Packaging.package config
Target "Packaging:Restore" <| Packaging.restore config
Target "Solution:Build"    <| Solution.build config
Target "Solution:Clean"    <| Solution.clean config
Target "Versioning:Update" <| Versioning.update config

"Solution:Clean"
    ==> "Packaging:Restore"
    ==> "Versioning:Update"
    ==> "Solution:Build"
    ==> "Packaging:Package"
    ==> "Default"

RunParameterTargetOrDefault "target" "Default"
