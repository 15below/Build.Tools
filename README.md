# Welcome to the Build.Tools project

Convention driven .net builds scripts written in F#.

## How To Use

We pull this repository into other repositories for building using the paket GitHub dependencies (https://fsprojects.github.io/Paket/github-dependencies.html)

By adding this dependency, you automatically (though paket magic) get FAKE.

You can then have a build script which does (assuming paket bootstrapper is in a .paket folder & your solution file is in the root of src)

```
.paket\paket.bootstrapper.exe --run restore
packages\FAKE\tools\FAKE.exe paket-files\15below\Build.Tools\Core.fsx "solution=src\MySolution.sln"
```

You can customise any of the .fsx files and copy them into "paket-files\15below\Build.Tools" as part of your build script.
    
## Maintainers

* @richard-green (Richard Green)
* @BlythMeister (Chris Blyth)
* @Yewridge (Bruce Keen)
* @zzdtri (Martyn Osborne)

## What does it do?

* Restores NuGet dependencies
* Updates assembly and nuget package versions (including setting pre-release NuGet versions if you're not building from the master branch on the build server)
* Builds your solution
* Packs any nuspec files associated with project files in the repository
* Runs NUnit tests in dlls named *.Tests.dll (if there are any)
* Runs a set of SpecFlow features in a dll named *.Features.dll (if there is one)
* Pushes nupkg files to a NuGet server (only if running on a build server)

Optionally it can also:

* Build any nuspec file found in a Deploy folder in the repository
* Push nupkg files generated from a Deploy folder nuspec to a NuGet server

Check the wiki for documentation.
