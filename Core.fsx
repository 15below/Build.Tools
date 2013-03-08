#r    "./fake/fakelib.dll"
#load "./Packaging.fsx"
#load "./Solution.fsx"

open Fake

let config = 
    Map.ofList [
        "configuration", environVarOrDefault "configuration" "Release"
        "output", environVarOrDefault "output" @"C:\Packages"
        "tools", environVar "tools"
        "solution", environVar "solution"
    ]

Target "Default"           <| DoNothing
Target "Packaging:Package" <| Packaging.package config
Target "Packaging:Restore" <| Packaging.restore config
Target "Solution:Build"    <| Solution.build config
Target "Solution:Clean"    <| Solution.clean config

"Solution:Clean"
    ==> "Packaging:Restore"
    ==> "Solution:Build"
    ==> "Packaging:Package"
    ==> "Default"

RunParameterTargetOrDefault "target" "Default"
