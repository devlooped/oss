oss template
===

For my new open source projects, this is the basic repository structure and build approach I'm going to use. 

## Goals

1. Trivial to apply and update via [dotnet-file](https://github.com/kzu/dotnet-file) with just two simple `dotnet` commands.
2. Repo instructions should just be: `dotnet restore & dotnet build & dotnet test` right on the repo root.

## Installing

After creating an empty repo (maybe with just a `readme.md`), just run:

```
dotnet file add https://github.com/devlooped/oss
```
 
This will fetch all files from this repo template and create a `.netconfig` file 
containing all the downloaded entries. 

At this point, you should add a `skip` value to the `.netconfig` file for the entries 
you don't want to keep up-to-date afterwards. The default skips would likely match 
the provided [.netconfig](.netconfig), plus any extra files you want to modify, for 
example:

```gitconfig
[file]
	url = https://github.com/devlooped/oss

# don't sync the .netconfig itself, to avoid a loop
[file ".netconfig"]
	url = https://github.com/devlooped/oss/blob/main/.netconfig
	skip

# readme is always customized for the project
[file "readme.md"]
	url = https://github.com/devlooped/oss/blob/main/readme.md
	skip

# we'll be tweaking the build, say
[file ".github/workflows/build.yml"]
	url = https://github.com/devlooped/oss/blob/main/.github/workflows/build.yml
	skip
 ```

> NOTE: you can also download the raw [.netconfig](.netconfig) from this repository 
> and run `dotnet file update` instead. It already contains skips for the readme.

## Updating

From this point on, applying template changes is as easy as running:

```
dotnet file update
```

You can also just list detected changes with:

```
dotnet file changes
```

Automation is provided via the [dotnet-file.yml](.github/workflows/dotnet-file.yml) 
workflow, which runs daily and does a `dotnet file sync` and creates a PR in your 
repository as needed, with a populated changelog to inspect the incoming changes.


## Design Choices

In no particular order:

1. `src` folder contains `Directory.Build.props` and `Directory.Build.targets` 
   and those contain all the customizations for the build, packaging and versioning. 
   In the past I went crazy factoring the targets into multiple files with single 
   purpose groupings and it bcomes [quite hard to follow](https://github.com/moq/moq/tree/a76c3cea6/src/build) 
   even for me, having written it all. So it's better to Keep Things Simple™.
   Logically related properties and items have a `Label` attribute as documentation.
   You can customize both by adding a `Directory.props` or `Directory.targets`, 
   which are imported at the end of both files.

2. If a `src/Directory.Packages.props` is found, I turn on 
   [centrally managed package versions](https://github.com/NuGet/Home/wiki/Centrally-managing-NuGet-package-versions), but it's not required.
    
3. GitHub Actions are provided for the CI/CD process as follows:
   - [Build](.github/workflows/build.yml): regular branch builds and PRs build. By default, build and test 
     jobs will run on `ubuntu-latest`. To customize this, create a `./.github/workflows/os-matrix.json` file in the 
     repository, with the matrix to use for the build, such as `["windows-latest", "ubuntu-latest", "macOS-latest"]`. 
   - [Changelog](.github/workflows/changelog.yml): when a release is released (not created, but actually released), 
     a changelog is calculated and pushed to main. The [changelog.config](.github/workflows/changelog.config) file 
     defines [changelog generation options](https://github.com/github-changelog-generator/github-changelog-generator/wiki/Advanced-change-log-generation-examples).
   - [Release notes](.github/workflows/release-notes.yml): when a release (either draft or final) is published, 
     the notes are generated using the same configuration above.
   - [Includes](.github/workflows/includes.yml): allows using HTML includes in markdown files for 
     easier content reuse. Readmes should include the 
     [standard footer](https://github.com/devlooped/sponsors/raw/main/footer.md) with:

     ```
     <!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
     ```

4. `dotnet format` is enforced on builds to keep consistency with `.editorconfig`.

5. [dependabot](.github/dependabot.yml) is configured to check for updated nuget packages daily.

6. A default [strong-name key](src/kzu.snk) is provided by default too. If the project does not desire to 
   strong-name the assemblies, it can be skipped as well in the `.netconfig` file. If present, the mentioned 
   `Directory.Build.*` targets will automatically pick the file and strong name assemblies.

7. [Bug](.github/ISSUE_TEMPLATE/bug.md) template provided. No addiitonal config provided since the 
   discussions URLs cannot be relative :(.
