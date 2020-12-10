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

At this point, you should add a `skip` value to the `.netconfig` file for the entries 
you don't want to keep up-to-date afterwards. The default skips would likely match 
the provided [.netconfig](.netconfig), plus any extra files you want to modify, for 
example:

```gitconfig
[file]
    url = https://github.com/kzu/oss
[file ".netconfig"]
    url = https://github.com/kzu/oss/blob/main/.netconfig
    skip
[file "readme.md"]
    url = https://github.com/kzu/oss/blob/main/readme.md
    skip
[file "src/icon.png"]
    url = https://github.com/kzu/oss/blob/main/src/icon.png
    skip
[file ".github/ISSUE_TEMPLATE/config.yml"]
    url = https://github.com/kzu/oss/blob/main/.github/ISSUE_TEMPLATE/config.yml
    skip
 ```

> NOTE: you can also download the raw [.netconfig](.netconfig) from this repository 
> and run `dotnet file update` instead. It already contains skips for readme and icon.

## Updating

From this point on, applying template changes is as easy as running:

```
dotnet file update
```

You can also just list detected changes with:

```
dotnet file changes
```

## Design Choices

In no particular order:

1. `src` folder contains `Directory.Build.props` and `Directory.Build.targets` 
   and those contain all the customizations for the build, packaging and versioning. 
   In the past I went crazy factoring the targets into multiple files with single 
   purpose groupings and it [quite hard to follow](https://github.com/moq/moq/tree/a76c3cea6/src/build) even for me, having written it all. So it's better to Keep Things Simple�.
   Logically related properties and items have a `Label` attribute as documentation.
   You can customize both by adding a `Directory.props` or `Directory.targets`, 
   which are imported at the end of both files.

2. If a `src/Directory.Packages.props` is found, I turn on 
   [centrally managed package versions](https://github.com/NuGet/Home/wiki/Centrally-managing-NuGet-package-versions), but it's not required. "Regular" versioning is more 
   friendly with (as in it actually works) dependabot at the moment.
    
3. GitHub Actions are provided for the CI/CD process as follows:
   - [Build](.github/workflows/build.yml): regular branch builds and PRs build, tested and packed on ubuntu-latest, 
     windows-latest and macOS-latest with `dotnet` build, test and pack respectively.
   - [Tags](.github/workflows/tag.yml): when a tag is pushed, a changelog is calculated from the previous tag 
     and used as the body of a draft GitHub release. If the tag contains a prerelease 
     label, the release is marked as such too. The [.github_changelog_generator](.github_changelog_generator) file 
     defines [changelog generation options](https://github.com/github-changelog-generator/github-changelog-generator/wiki/Advanced-change-log-generation-examples).
   - [Release](.github/workflows/release.yml): the draft release can be reviewed and edited and when ready, published. 
     At this point, the release.yml workflow 

4. `dotnet format` is enforced on builds to keep consistency with `.editorconfig`.

5. [dependabot](.github/dependabot.yml) is configured to check for updated nuget packages every week.

6. A default [icon.png](src/icon.png) and strong-name key are provided by default too. These may be skipped 
   as well in the `.netconfig` file if the intention is to customize them for a particular project.

7. [Bug](.github/ISSUE_TEMPLATE/bug.md) and [Feature](.github/ISSUE_TEMPLATE/feature.md) issue templates 
   are provided.
