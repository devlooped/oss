kzu's ultimate oss structure
===

For my new open source projects, this is the basic repository structure and build approach I'm going to use. 

## Goals

1. Easy to update via [dotnet-file](https://github.com/kzu/dotnet-file)
2. Repo instructions should just be: `dotnet restore & dotnet build & dotnet test` right on the repo root.

## Installing

After creating an empty repo (maybe with just a `README.md`), download the [.netconfig](.netconfig) file to the new repository root and run:

```
dotnet file update
```
 
Optionally remove from your copy of the `.netconfig` file the entries you don't want to keep up-to-date 
afterwards. A typical one to remove if you make changes to it, would be `azure-pipelines.yml`. But since 
everything will be source controlled, at the time you update, you can review what changed and whether 
you want to commit those changes in a case by case basis.

## Updating

```
dotnet file update
```
