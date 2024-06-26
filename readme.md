![logo](https://github.com/devlooped/devlooped.github.io/blob/main/img/tiny.png) oss template
===

For my new open source projects, this is the basic repository structure and build approach I'm going to use. 

## Goals

1. Trivial to apply and update via [dotnet-file](https://github.com/kzu/dotnet-file) with just two simple `dotnet` commands.
2. Repo instructions should just be: `dotnet restore & dotnet build & dotnet test` right on the repo root.

## Installing

After creating an empty repo (maybe with just a `readme.md`), just run:

```
dotnet file init https://github.com/devlooped/oss/blob/main/.netconfig
```
 
This will fetch the given [dotnetconfig](https://dotnetconfig.org] and
synchronize the configured files and create a [.netconfig](.netconfig) 
in the repo containing all the downloaded entries for future sync.

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
   purpose groupings and it bcomes [quite hard to follow](https://github.com/devlooped/moq/tree/a76c3cea6/src/build) 
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
# Sponsors 

<!-- sponsors.md -->
[![Clarius Org](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/clarius.png "Clarius Org")](https://github.com/clarius)
[![Kirill Osenkov](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/KirillOsenkov.png "Kirill Osenkov")](https://github.com/KirillOsenkov)
[![MFB Technologies, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/MFB-Technologies-Inc.png "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![Stephen Shaw](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/decriptor.png "Stephen Shaw")](https://github.com/decriptor)
[![Torutek](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/torutek-gh.png "Torutek")](https://github.com/torutek-gh)
[![DRIVE.NET, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/drivenet.png "DRIVE.NET, Inc.")](https://github.com/drivenet)
[![Ashley Medway](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/AshleyMedway.png "Ashley Medway")](https://github.com/AshleyMedway)
[![Keith Pickford](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Keflon.png "Keith Pickford")](https://github.com/Keflon)
[![Thomas Bolon](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/tbolon.png "Thomas Bolon")](https://github.com/tbolon)
[![Kori Francis](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/kfrancis.png "Kori Francis")](https://github.com/kfrancis)
[![Toni Wenzel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/twenzel.png "Toni Wenzel")](https://github.com/twenzel)
[![Giorgi Dalakishvili](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Giorgi.png "Giorgi Dalakishvili")](https://github.com/Giorgi)
[![Uno Platform](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/unoplatform.png "Uno Platform")](https://github.com/unoplatform)
[![Dan Siegel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/dansiegel.png "Dan Siegel")](https://github.com/dansiegel)
[![Reuben Swartz](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/rbnswartz.png "Reuben Swartz")](https://github.com/rbnswartz)
[![Jacob Foshee](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jfoshee.png "Jacob Foshee")](https://github.com/jfoshee)
[![](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Mrxx99.png "")](https://github.com/Mrxx99)
[![Eric Johnson](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/eajhnsn1.png "Eric Johnson")](https://github.com/eajhnsn1)
[![Ix Technologies B.V.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/IxTechnologies.png "Ix Technologies B.V.")](https://github.com/IxTechnologies)
[![David JENNI](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/davidjenni.png "David JENNI")](https://github.com/davidjenni)
[![Jonathan ](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Jonathan-Hickey.png "Jonathan ")](https://github.com/Jonathan-Hickey)
[![Oleg Kyrylchuk](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/okyrylchuk.png "Oleg Kyrylchuk")](https://github.com/okyrylchuk)
[![Charley Wu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/akunzai.png "Charley Wu")](https://github.com/akunzai)
[![Jakob Tikjøb Andersen](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jakobt.png "Jakob Tikjøb Andersen")](https://github.com/jakobt)
[![Seann Alexander](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/seanalexander.png "Seann Alexander")](https://github.com/seanalexander)
[![Tino Hager](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/tinohager.png "Tino Hager")](https://github.com/tinohager)
[![Mark Seemann](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/ploeh.png "Mark Seemann")](https://github.com/ploeh)
[![Ken Bonny](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/KenBonny.png "Ken Bonny")](https://github.com/KenBonny)
[![Simon Cropp](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/SimonCropp.png "Simon Cropp")](https://github.com/SimonCropp)
[![agileworks-eu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/agileworks-eu.png "agileworks-eu")](https://github.com/agileworks-eu)
[![sorahex](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/sorahex.png "sorahex")](https://github.com/sorahex)
[![Zheyu Shen](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/arsdragonfly.png "Zheyu Shen")](https://github.com/arsdragonfly)
[![Vezel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/vezel-dev.png "Vezel")](https://github.com/vezel-dev)
[![ChilliCream](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/ChilliCream.png "ChilliCream")](https://github.com/ChilliCream)
[![4OTC](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/4OTC.png "4OTC")](https://github.com/4OTC)


<!-- sponsors.md -->

[![Sponsor this project](https://raw.githubusercontent.com/devlooped/sponsors/main/sponsor.png "Sponsor this project")](https://github.com/sponsors/devlooped)
&nbsp;

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
     ```

4. `dotnet format` is enforced on builds to keep consistency with `.editorconfig`.

5. [dependabot](.github/dependabot.yml) is configured to check for updated nuget packages daily.

6. A default [strong-name key](src/kzu.snk) is provided by default too. If the project does not desire to 
   strong-name the assemblies, it can be skipped as well in the `.netconfig` file. If present, the mentioned 
   `Directory.Build.*` targets will automatically pick the file and strong name assemblies.

7. [Bug](.github/ISSUE_TEMPLATE/bug.md) template provided. No addiitonal config provided since the 
   discussions URLs cannot be relative :(.
