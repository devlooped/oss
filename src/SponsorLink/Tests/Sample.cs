extern alias Analyzer;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Analyzer::Devlooped.Sponsors;
using Xunit;
using Xunit.Abstractions;

namespace Tests;

public class Sample(ITestOutputHelper output)
{
    [Theory]
    [InlineData("es-AR", DiagnosticKind.Unknown)]
    [InlineData("es-AR", DiagnosticKind.Expiring)]
    [InlineData("es-AR", DiagnosticKind.Expired)]
    [InlineData("es-AR", DiagnosticKind.Sponsor)]
    [InlineData("en", DiagnosticKind.Unknown)]
    [InlineData("en", DiagnosticKind.Expiring)]
    [InlineData("en", DiagnosticKind.Expired)]
    [InlineData("en", DiagnosticKind.Sponsor)]
    [InlineData("", DiagnosticKind.Unknown)]
    [InlineData("", DiagnosticKind.Expiring)]
    [InlineData("", DiagnosticKind.Expired)]
    [InlineData("", DiagnosticKind.Sponsor)]
    public void Test(string culture, DiagnosticKind kind)
    {
        Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = 
            culture == "" ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(culture);

        var diag = new DiagnosticsManager().GetDescriptor(["foo"], "bar", "FB", kind);

        output.WriteLine(diag.Title.ToString());
        output.WriteLine(diag.MessageFormat.ToString());
        output.WriteLine(diag.Description.ToString());
    }

    [Fact]
    public void RenderSponsorables()
    {
        Assert.NotEmpty(SponsorLink.Sponsorables);

        foreach (var pair in SponsorLink.Sponsorables)
        {
            output.WriteLine($"{pair.Key} = {pair.Value}");
            // Read the JWK
            var jsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey.Create(pair.Value);

            Assert.NotNull(jsonWebKey);

            using var key = RSA.Create(new RSAParameters
            {
                Modulus = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.DecodeBytes(jsonWebKey.N),
                Exponent = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.DecodeBytes(jsonWebKey.E),
            });
        }
    }
}