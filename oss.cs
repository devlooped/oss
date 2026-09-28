// First run: dnx runfile https://github.com/devlooped/oss/blob/main/oss.cs --yes --alias oss
// Subsequently: dnx runfile oss --yes
#:package Spectre.Console@*
#:package CliWrap@*
#:package ConsoleAppFramework@*

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using CliWrap;
using ConsoleAppFramework;
using Spectre.Console;

await ConsoleApp.RunAsync(args, async (
    /// <summary>Project name</summary>
    string? project = null,
    /// <summary>Repo name (defaults to project name)</summary>
    string? repo = null,
    /// <summary>Package ID (defaults to Devlooped.{project})</summary>
    string? package = null) =>
{
    // Framework-dependent / R2R: muxer is three levels above the shared runtime.
    // Native AOT (e.g. dnx go default publish) reports the app dir as the runtime
    // directory, so that walk is wrong — use DOTNET_HOST_PATH / DOTNET_ROOT / PATH.
    var fileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";
    var dotnet = Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", "..", fileName));
    if (!File.Exists(dotnet))
    {
        if (Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") is { Length: > 0 } host && File.Exists(host))
            dotnet = host;
        else if (Environment.GetEnvironmentVariable("DOTNET_ROOT") is { Length: > 0 } root
            && File.Exists(Path.Combine(root, fileName)))
            dotnet = Path.Combine(root, fileName);
        else
            dotnet = fileName; // PATH
    }

    AnsiConsole.Write(new FigletText("devlooped oss").Color(Color.Green));
    AnsiConsole.WriteLine();

    var projectName = project ?? AnsiConsole.Prompt(
        new TextPrompt<string>("[green]Project name[/]:")
            .PromptStyle("yellow")
            .ValidationErrorMessage("[red]Project name cannot be empty[/]")
            .Validate(v => !string.IsNullOrWhiteSpace(v)));

    var repoName = repo ?? AnsiConsole.Prompt(
        new TextPrompt<string>("[green]Repo name[/]:")
            .PromptStyle("yellow")
            .DefaultValue(projectName));

    var packageId = package ?? AnsiConsole.Prompt(
        new TextPrompt<string>("[green]Package ID[/]:")
            .PromptStyle("yellow")
            .DefaultValue($"Devlooped.{projectName}"));

    AnsiConsole.WriteLine();
    AnsiConsole.Write(new Rule("[yellow]Setting up OSS project[/]").RuleStyle("grey").LeftJustified());
    AnsiConsole.WriteLine();

    var projectFile = $"src/{projectName}/{projectName}.csproj";
    var testsFile = "src/Tests/Tests.csproj";
    var solutionFile = $"{projectName}.slnx";

    await RunDotNet(
        "file init https://github.com/devlooped/oss/blob/main/.netconfig",
        "Initializing dotnet file sync from devlooped/oss",
        [".netconfig"]);

    if (await RunDotNet(
        $"new classlib -n {projectName} -o src/{projectName} -f net10.0",
        $"Creating class library src/{projectName}",
        [projectFile]))
        File.Delete($"src/{projectName}/Class1.cs");

    await RunDotNet(
        $"package add NuGetizer --project {projectFile}",
        "Adding NuGetizer",
        skip: HasPackageReference(projectFile, "NuGetizer"));

    if (await RunDotNet(
        $"new xunit -n Tests -o src/Tests -f net10.0",
        "Creating xUnit test project src/Tests",
        [testsFile]))
        File.Delete($"src/Tests/UnitTest1.cs");

    await RunDotNet(
        $"add {testsFile} reference {projectFile}",
        $"Adding reference from Tests to {projectName}",
        skip: HasProjectReference(testsFile, projectFile));

    EnsureProject(projectFile, packageId);
    EnsureProject(testsFile, testProject: true);

    await WriteIfMissing(
        "src/Directory.props",
        $"""
        <Project>
          <PropertyGroup>
            <Product>{projectName}</Product>
            <ImplicitUsings>true</ImplicitUsings>
          </PropertyGroup>
        </Project>
        """,
        "Creating src/Directory.props");

    await RunDotNet(
        $"new solution -n {projectName}",
        $"Creating solution {solutionFile}",
        [solutionFile]);

    var missingProjects = ProjectsMissingFromSolution(solutionFile, projectFile, testsFile);
    if (missingProjects.Length == 0)
        Skip($"Adding projects to {solutionFile}");
    else
        await RunDotNet(
            $"sln {solutionFile} add --in-root {string.Join(' ', missingProjects)}",
            $"Adding projects to {solutionFile}");

    if (File.Exists("readme.md"))
    {
        AnsiConsole.MarkupLine("[green]✓[/] Created [yellow]readme.md[/] [grey](already exists)[/]");
    }
    else
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("green"))
            .StartAsync("Downloading readme.md template...", async ctx =>
            {
                using var http = new HttpClient();
                var readmeContent = await http.GetStringAsync(
                    "https://raw.githubusercontent.com/devlooped/oss/main/readme.tmp.md");

                readmeContent = readmeContent
                    .Replace("{{PROJECT_NAME}}", projectName)
                    .Replace("{{PACKAGE_ID}}", packageId)
                    .Replace("{{REPO_NAME}}", repoName);

                await File.WriteAllTextAsync("readme.md", readmeContent);
                ctx.Status("Downloaded and processed readme.md");
            });

        AnsiConsole.MarkupLine("[green]✓[/] Created [yellow]readme.md[/]");
    }

    await WriteIfMissing(
        $"src/{projectName}/readme.md",
        $"""
        [![EULA](https://img.shields.io/badge/EULA-OSMF-blue?labelColor=black&color=C9FF30)](osmfeula.txt)
        [![OSS](https://img.shields.io/github/license/devlooped/oss.svg?color=blue)](license.txt)
        [![GitHub](https://img.shields.io/badge/-source-181717.svg?logo=GitHub)](https://github.com/devlooped/{repoName})

        <!-- include ../../readme.md#content -->

        <!-- include https://github.com/devlooped/.github/raw/main/osmf.md -->

        <!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->

        <!-- exclude -->
        """,
        $"Creating src/{projectName}/readme.md");

    AnsiConsole.WriteLine();
    AnsiConsole.Write(new Rule("[green]Done![/]").RuleStyle("grey").LeftJustified());
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine($"[bold]Project:[/] [yellow]{projectName}[/]");
    AnsiConsole.MarkupLine($"[bold]Repo:[/] [yellow]{repoName}[/]");
    AnsiConsole.MarkupLine($"[bold]Package ID:[/] [yellow]{packageId}[/]");

    // Returns true when the command ran. Existing output files, or skip: true, leave the step alone.
    async Task<bool> RunDotNet(string command, string description, string[]? outputs = null, bool skip = false)
    {
        if (skip || (outputs is { Length: > 0 } && Array.TrueForAll(outputs, File.Exists)))
        {
            Skip(description);
            return false;
        }

        AnsiConsole.MarkupLine($"[grey]{Markup.Escape(description)}...[/]");
        await Cli.Wrap(dotnet)
            .WithArguments(command)
            .WithStandardOutputPipe(PipeTarget.ToStream(Console.OpenStandardOutput()))
            .WithStandardErrorPipe(PipeTarget.ToStream(Console.OpenStandardError()))
            .ExecuteAsync();
        AnsiConsole.MarkupLine($"[green]✓[/] {Markup.Escape(description)}");
        return true;
    }

    async Task WriteIfMissing(string path, string contents, string description)
    {
        if (File.Exists(path))
        {
            Skip(description);
            return;
        }

        await File.WriteAllTextAsync(path, contents);
        AnsiConsole.MarkupLine($"[green]✓[/] {Markup.Escape(description)}");
    }

    void Skip(string description) =>
        AnsiConsole.MarkupLine($"[green]✓[/] {Markup.Escape(description)} [grey](already exists)[/]");

    bool HasPackageReference(string project, string packageId)
    {
        if (!File.Exists(project))
            return false;

        foreach (var element in XDocument.Load(project).Descendants("PackageReference"))
        {
            var name = element.Attribute("Include")?.Value ?? element.Attribute("Update")?.Value;
            if (string.Equals(name, packageId, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    bool HasProjectReference(string project, string referencedProject)
    {
        if (!File.Exists(project))
            return false;

        var referencedName = Path.GetFileName(referencedProject);
        foreach (var element in XDocument.Load(project).Descendants("ProjectReference"))
        {
            var include = element.Attribute("Include")?.Value;
            if (include is not null &&
                string.Equals(Path.GetFileName(include), referencedName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    string[] ProjectsMissingFromSolution(string solution, params string[] projects)
    {
        if (!File.Exists(solution))
            return projects;

        var text = File.ReadAllText(solution);
        var missing = new List<string>();
        foreach (var project in projects)
        {
            var forward = project.Replace('\\', '/');
            var backward = project.Replace('/', '\\');
            if (!text.Contains(forward, StringComparison.OrdinalIgnoreCase) &&
                !text.Contains(backward, StringComparison.OrdinalIgnoreCase))
                missing.Add(project);
        }

        return [.. missing];
    }

    void EnsureProject(string path, string? packageId = null, bool testProject = false)
    {
        var doc = XDocument.Load(path, LoadOptions.None);
        var group = doc.Root?.Element("PropertyGroup");
        if (group is null)
            return;

        var changed = false;
        changed |= RemoveElement(group, "ImplicitUsings");
        changed |= RemoveElement(group, "Nullable");
        if (testProject)
            changed |= RemoveElement(group, "IsPackable");

        if (packageId is not null && group.Element("PackageId") is null)
        {
            group.Add(new XElement("PackageId", packageId));
            changed = true;
        }

        if (changed)
            doc.Save(path);

        static bool RemoveElement(XElement parent, string name)
        {
            if (parent.Element(name) is not { } element)
                return false;

            element.Remove();
            return true;
        }
    }
});