﻿// <autogenerated />
namespace Devlooped.Sponsors;

/// <summary>
/// The kind of SponsorLink diagnostic being reported.
/// </summary>
public enum DiagnosticKind
{
    /// <summary>
    /// Sponsorship status is unknown.
    /// </summary>
    Unknown,
    /// <summary>
    /// The sponsors manifest is expired but within the grace period.
    /// </summary>
    Expiring,
    /// <summary>
    /// The sponsors manifest is expired and outside the grace period.
    /// </summary>
    Expired,
    /// <summary>
    /// The user is sponsoring.
    /// </summary>
    Sponsor,
}
