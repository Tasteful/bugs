# dnu pack will fail if consuming nuget package `ServiceBus.V1_1`

Sample application for https://github.com/aspnet/dnx/issues/3294

Build from VS or run `dnu pack` to reproduce the error.

Nuget pack is working, testew with the included nuspec.
Execute the following from the src\ClassLibrary1-folder.

`nuget pack ClassLibrary1.nuspec`