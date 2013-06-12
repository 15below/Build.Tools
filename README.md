# Welcome to the Build.Tools project

Convention driven .net builds scripts written in F#. Sub-module this repository at the root of your code repository and set 2 environment (or command line) variables and you're good to go.

For example, assuming you submodule this repository into a directory called tools:

    tools\Fake\FAKE.exe tools\Core.fsx "tools=tools" "solution=src\MySolution.sln"
    
## What does it do?

* Restores NuGet dependencies
* Updates assembly and nuget package versions (including setting pre-release NuGet versions if you're not building from the master branch on the build server)
* Builds your solution
* Packs any nuspec files associated with project files in the repository
* Runs NUnit tests in dlls named *.Tests.dll (if there are any)
* Runs a set of SpecFlow features in a dll named *.Features.dll (if there is one)
* Pushes nupkg files to a NuGet server (only if running on a build server)

Check the wiki for documentation.
