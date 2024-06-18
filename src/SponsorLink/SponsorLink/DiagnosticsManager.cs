﻿// <autogenerated />
#nullable enable
using System;
using System.Collections.Concurrent;
using System.Globalization;
using Humanizer;
using Microsoft.CodeAnalysis;

namespace Devlooped.Sponsors;

/// <summary>
/// Manages diagnostics for the SponsorLink analyzer so that there are no duplicates 
/// when multiple projects share the same product name (i.e. ThisAssembly).
/// </summary>
class DiagnosticsManager
{
    /// <summary>
    /// Acceses the diagnostics dictionary for the current <see cref="AppDomain"/>.
    /// </summary>
    ConcurrentDictionary<string, Diagnostic> Diagnostics
    {
        get => AppDomainDictionary.Get<ConcurrentDictionary<string, Diagnostic>>(nameof(Diagnostics));
    }

    /// <summary>
    /// Creates a descriptor from well-known diagnostic kinds.
    /// </summary>
    /// <param name="sponsorable">The names of the sponsorable accounts that can be funded for the given product.</param>
    /// <param name="product">The product or project developed by the sponsorable(s).</param>
    /// <param name="prefix">Custom prefix to use for diagnostic IDs.</param>
    /// <param name="status">The kind of status diagnostic to create.</param>
    /// <returns>The given <see cref="DiagnosticDescriptor"/>.</returns>
    /// <exception cref="NotImplementedException">The <paramref name="status"/> is not one of the known ones.</exception>
    public DiagnosticDescriptor GetDescriptor(string[] sponsorable, string product, string prefix, SponsorStatus status) => status switch
    {
        SponsorStatus.Unknown => CreateUnknown(sponsorable, product, prefix),
        SponsorStatus.Sponsor => CreateSponsor(sponsorable, prefix),
        SponsorStatus.Expiring => CreateExpiring(sponsorable, prefix),
        SponsorStatus.Expired => CreateExpired(sponsorable, prefix),
        _ => throw new NotImplementedException(),
    };

    /// <summary>
    /// Pushes a diagnostic for the given product. If an existing one exists, it is replaced.
    /// </summary>
    /// <returns>The same diagnostic that was pushed, for chained invocations.</returns>
    public Diagnostic Push(string product, Diagnostic diagnostic)
    {
        // Directly sets, since we only expect to get one warning per sponsorable+product 
        // combination.
        Diagnostics[product] = diagnostic;
        return diagnostic;
    }

    /// <summary>
    /// Attemps to remove a diagnostic for the given product.
    /// </summary>
    /// <param name="product">The product diagnostic that might have been pushed previously.</param>
    /// <returns>The removed diagnostic, or <see langword="null" /> if none was previously pushed.</returns>
    public Diagnostic? Pop(string product)
    {
        Diagnostics.TryRemove(product, out var diagnostic);
        return diagnostic;
    }

    /// <summary>
    /// Gets the status of the given product based on a previously stored diagnostic.
    /// </summary>
    /// <param name="product">The product to check status for.</param>
    /// <returns>Optional <see cref="SponsorStatus"/> that was reported, if any.</returns>
    public SponsorStatus? GetStatus(string product)
    {
        // NOTE: the SponsorLinkAnalyzer.SetStatus uses diagnostic properties to store the 
        // kind of diagnostic as a simple string instead of the enum. We do this so that 
        // multiple analyzers or versions even across multiple products, which all would 
        // have their own enum, can still share the same diagnostic kind.
        if (Diagnostics.TryGetValue(product, out var diagnostic) &&
            diagnostic.Properties.TryGetValue(nameof(SponsorStatus), out var value))
        {
            // Switch on value matching DiagnosticKind names
            return value switch
            {
                nameof(SponsorStatus.Unknown) => SponsorStatus.Unknown,
                nameof(SponsorStatus.Sponsor) => SponsorStatus.Sponsor,
                nameof(SponsorStatus.Expiring) => SponsorStatus.Expiring,
                nameof(SponsorStatus.Expired) => SponsorStatus.Expired,
                _ => null,
            };
        }

        return null;
    }

    static DiagnosticDescriptor CreateSponsor(string[] sponsorable, string prefix) => new(
        $"{prefix}100",
        Resources.Sponsor_Title,
        Resources.Sponsor_Message,
        "SponsorLink",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Resources.Sponsor_Description,
        helpLinkUri: "https://github.com/devlooped#sponsorlink",
        "DoesNotSupportF1Help");

    static DiagnosticDescriptor CreateUnknown(string[] sponsorable, string product, string prefix) => new(
        $"{prefix}101",
        Resources.Unknown_Title,
        Resources.Unknown_Message,
        "SponsorLink",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: string.Format(CultureInfo.CurrentCulture, Resources.Unknown_Description,
            sponsorable.Humanize(x => $"https://github.com/sponsors/{x}"),
            string.Join(" ", sponsorable)),
        helpLinkUri: "https://github.com/devlooped#sponsorlink",
        WellKnownDiagnosticTags.NotConfigurable);

    static DiagnosticDescriptor CreateExpiring(string[] sponsorable, string prefix) => new(
         $"{prefix}103",
         Resources.Expiring_Title,
         Resources.Expiring_Message,
         "SponsorLink",
         DiagnosticSeverity.Warning,
         isEnabledByDefault: true,
         description: string.Format(CultureInfo.CurrentCulture, Resources.Expiring_Description, string.Join(" ", sponsorable)),
         helpLinkUri: "https://github.com/devlooped#autosync",
         "DoesNotSupportF1Help", WellKnownDiagnosticTags.NotConfigurable);

    static DiagnosticDescriptor CreateExpired(string[] sponsorable, string prefix) => new(
         $"{prefix}104",
         Resources.Expired_Title,
         Resources.Expired_Message,
         "SponsorLink",
         DiagnosticSeverity.Warning,
         isEnabledByDefault: true,
         description: string.Format(CultureInfo.CurrentCulture, Resources.Expired_Description, string.Join(" ", sponsorable)),
         helpLinkUri: "https://github.com/devlooped#autosync",
         "DoesNotSupportF1Help", WellKnownDiagnosticTags.NotConfigurable);
}
