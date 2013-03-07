#r    "./fake/fakelib.dll"
#load "./Packaging.fsx"
#load "./Solution.fsx"

open Fake

let tools = environVar "tools"
let solution = environVar "solution"

Target "Default"           <| DoNothing
Target "Packaging:Restore" <| Packaging.restore tools
Target "Solution:Build"    <| Solution.build solution
Target "Solution:Clean"    <| Solution.clean solution

"Solution:Clean"
    ==> "Packaging:Restore"
    ==> "Solution:Build"
    ==> "Default"

RunParameterTargetOrDefault "target" "Default"
