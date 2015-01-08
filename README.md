kzu's ultimate oss structure
===

For my new open source projects, this is the basic repository structure and build approach I'm going to use. 

## Goals

1. [Automatic NuGet Restore](http://www.cazzulino.com/ultimate-cross-platform-nuget-restore.html): should work automatically for IDE builds, msbuild and CI builds. 
2. Uncluttered repository root: the goal is for someone landing on the repo root in GitHub to be able to see the Readme right-away, without having to scroll past a pile of (up-front) irrelevant files. All source files under `src`, all build-related reusable assets under `build`, add `docs`, `design` as needed, but no loose files in the root directory, with the sole exception of the `LICENSE`, the `README` and the `build.proj` build script.
3. Cloning and running just `mbuild` or `xbuild` should Just Work.

## Features

1. Customizable build.proj: add `@(Solution)`, `@(Project)` and `@(NuSpec)` to build stuff.
2. Blank solution starting point: getting the desired folder structure from VS' New Project dialog can be tricky. An empty solution helps starting with the right folder structure and you can place the individual projects directly under `src` or inside nested per-area subfolder if desired, adding them to the blank solution. Just renaming the solution from within VS is all that's needed. The build script keeps building it as it's imported via a `src\*.sln` wildcard.

## Installing

The easiest way is to just create a new repository, and install the NuGet package from a command line with NuGet.exe in the path, right in your repository root folder:

	NuGet Install build -ExcludeVersion

This will bring the content from this repository's [build](https://github.com/kzu/oss/tree/master/build) folder as well as the sample solution from [src](https://github.com/kzu/oss/tree/master/src) and the [build.proj](https://github.com/kzu/oss/blob/master/build.proj) sample build file.

Anternatively, you can just clone this repo and push it to yours right when the project is starting. 
 
## Updating

Because the build artifacts have been turned into a nuget, you can keep it up to date by simply re-running the Install command again as explained above :).


