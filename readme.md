kzu's oss
===

For my new open source projects, this is the basic repository structure and build approach I'm going to use. 

## Goals

1. Trivial to apply and update via [dotnet-file](https://github.com/kzu/dotnet-file) with just two simple `dotnet` commands.
2. Repo instructions should just be: `dotnet restore & dotnet build & dotnet test` right on the repo root.

## Installing

After creating an empty repo (maybe with just a `readme.md`), just run:

```
dotnet file add https://github.com/kzu/oss
```
 
This will fetch all files from this repo template and create a `.netconfig` file 
containing all the downloaded entries. 

At this point, you should remove from the `.netconfig` file the entries you don't 
want to keep up-to-date afterwards, such as the `readme.md`. In any case, since 
everything will be source controlled in your repository, at the time you update, 
you can review what changed and whether you want to commit those changes in a case 
by case basis.

## Updating

From this point on, applying template changes is as easy as running:

```
dotnet file update
```

You can also just list detected changes with:

```
dotnet file changes
```