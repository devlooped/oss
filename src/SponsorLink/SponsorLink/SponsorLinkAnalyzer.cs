using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using static Devlooped.Sponsors.SponsorLink;
using static ThisAssembly.Constants;

namespace Devlooped.Sponsors;

/// <summary>
/// Links the sponsor status for the current compilation.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
public class SponsorLinkAnalyzer : DiagnosticAnalyzer
{
    static readonly int graceDays = int.Parse(Funding.Grace);
    static readonly Dictionary<DiagnosticKind, DiagnosticDescriptor> descriptors = new()
    {
        // Requires:
        // <Constant Include="Funding.Product" Value="[PRODUCT_NAME]" />
        // <Constant Include="Funding.AnalyzerPrefix" Value="[PREFIX]" />
        { DiagnosticKind.Unknown, Diagnostics.GetDescriptor([.. Sponsorables.Keys], Funding.Product, Funding.Prefix, DiagnosticKind.Unknown) },
        { DiagnosticKind.Sponsor, Diagnostics.GetDescriptor([.. Sponsorables.Keys], Funding.Product, Funding.Prefix, DiagnosticKind.Sponsor) },
        { DiagnosticKind.Expiring, Diagnostics.GetDescriptor([.. Sponsorables.Keys], Funding.Product, Funding.Prefix, DiagnosticKind.Expiring) },
        { DiagnosticKind.Expired, Diagnostics.GetDescriptor([.. Sponsorables.Keys], Funding.Product, Funding.Prefix, DiagnosticKind.Expired) },
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = descriptors.Values.ToImmutableArray();

#pragma warning disable RS1026 // Enable concurrent execution
    public override void Initialize(AnalysisContext context)
#pragma warning restore RS1026 // Enable concurrent execution
    {
#if !DEBUG
        // Only enable concurrent execution in release builds, otherwise debugging is quite annoying.
        context.EnableConcurrentExecution();
#endif
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

#pragma warning disable RS1013 // Start action has no registered non-end actions
        // We do this so that the status is set at compilation start so we can use it 
        // across all other analyzers. We report only on finish because multiple 
        // analyzers can report the same diagnostic and we want to avoid duplicates. 
        context.RegisterCompilationStartAction(ctx =>
        {
            var manifests = ctx.Options.AdditionalFiles
                .Where(x =>
                    ctx.Options.AnalyzerConfigOptionsProvider.GetOptions(x).TryGetValue("build_metadata.AdditionalFiles.SourceItemType", out var itemType) &&
                    itemType == "SponsorManifest" &&
                    Sponsorables.ContainsKey(Path.GetFileNameWithoutExtension(x.Path)))
                .ToImmutableArray();

            // Setting the status early allows other analyzers to potentially check for it.
            var status = SetStatus(manifests);
            // Never report any diagnostic unless we're in an editor.
            if (IsEditor)
            {
                // NOTE: even if we don't report the diagnostic, we still set the status so other analyzers can use it.
                ctx.RegisterCompilationEndAction(ctx =>
                {
                    if (Diagnostics.Pop(Funding.Product) is Diagnostic diagnostic)
                    {
                        ctx.ReportDiagnostic(diagnostic);
                    }
                    else
                    {
                        // This should never happen and would be a bug.
                        Debug.Assert(true, "We should have provided a diagnostic of some kind for " + Funding.Product);
                        // We'll report it as unknown as a fallback for now.
                        ctx.ReportDiagnostic(Diagnostic.Create(descriptors[DiagnosticKind.Unknown], null,
                            properties: ImmutableDictionary.Create<string, string?>().Add(nameof(DiagnosticKind), nameof(DiagnosticKind.Unknown)),
                            Funding.Product, Sponsorables.Keys.Humanize(ThisAssembly.Strings.Or)));
                    }
                });
            }
        });
#pragma warning restore RS1013 // Start action has no registered non-end actions
    }

    DiagnosticKind SetStatus(ImmutableArray<AdditionalText> manifests)
    {
        if (!Manifest.TryRead(out var claims, manifests.Select(text => 
                (text.GetText()?.ToString() ?? "", Sponsorables[Path.GetFileNameWithoutExtension(text.Path)]))) ||
            claims.GetExpiration() is not DateTime exp)
        {
            // report unknown, either unparsed manifest or one with no expiration (which we never emit).
            Diagnostics.Push(Funding.Product, Diagnostic.Create(descriptors[DiagnosticKind.Unknown], null,
                properties: ImmutableDictionary.Create<string, string?>().Add(nameof(DiagnosticKind), nameof(DiagnosticKind.Unknown)),
                Funding.Product, Sponsorables.Keys.Humanize(ThisAssembly.Strings.Or)));
            return DiagnosticKind.Unknown;
        }
        else if (exp < DateTime.Now)
        {
            // report expired or expiring soon if still within the configured days of grace period
            if (exp.AddDays(graceDays) < DateTime.Now)
            {
                // report expiring soon
                Diagnostics.Push(Funding.Product, Diagnostic.Create(descriptors[DiagnosticKind.Expiring], null,
                    properties: ImmutableDictionary.Create<string, string?>().Add(nameof(DiagnosticKind), nameof(DiagnosticKind.Expiring))));
                return DiagnosticKind.Expiring;
            }
            else
            {
                // report expired
                Diagnostics.Push(Funding.Product, Diagnostic.Create(descriptors[DiagnosticKind.Expired], null,
                    properties: ImmutableDictionary.Create<string, string?>().Add(nameof(DiagnosticKind), nameof(DiagnosticKind.Expired))));
                return DiagnosticKind.Expired;
            }
        }
        else
        {
            // report sponsor
            Diagnostics.Push(Funding.Product, Diagnostic.Create(descriptors[DiagnosticKind.Sponsor], null,
                properties: ImmutableDictionary.Create<string, string?>().Add(nameof(DiagnosticKind), nameof(DiagnosticKind.Sponsor)),
                Funding.Product));
            return DiagnosticKind.Sponsor;
        }
    }
}
