#r    "./fake/fakelib.dll"
#load "./Packaging.fsx"
#load "./Solution.fsx"
#load "./Versioning.fsx"

open Fake

let config = 
    Map.ofList [
        "build:configuration",   environVarOrDefault "configuration"         "Release"
        "build:solution",        environVar          "solution"
        "core:tools",            environVar          "tools"
        "packaging:output",      environVarOrDefault "output"                "C:/Packages"
        "versioning:version",    environVarOrDefault "core_version_number"   "0.0.0"
        "versioning:build",      environVarOrDefault "build_number"          "0"
        "versioning:branch:ci",  environVarOrDefault "teamcity_build_branch" "development"
        "versioning:branch:vcs", environVarOrDefault "vcsroot_branch"        (sprintf "%s/release" (environVarOrDefault "core_version_number" "0.0.0"))
    ]

Target "Default"           <| DoNothing
Target "Packaging:Package" <| Packaging.package config
Target "Packaging:Restore" <| Packaging.restore config
Target "Solution:Build"    <| Solution.build config
Target "Solution:Clean"    <| Solution.clean config
Target "Versioning:Update" <| Versioning.update config

//"Solution:Clean"
    //==> "Packaging:Restore"
"Versioning:Update"
    //==> "Solution:Build"
    //==> "Packaging:Package"
    ==> "Default"

RunParameterTargetOrDefault "target" "Default"
