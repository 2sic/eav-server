﻿using ToSic.Eav.Apps.Sys;

using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Sys.Insights.HtmlHelpers;
using ToSic.Razor.Markup;
using ToSic.Sys.Capabilities.Fingerprints;
using ToSic.Sys.Capabilities.Licenses;
using static ToSic.Eav.Sys.Insights.HtmlHelpers.InsightsHtmlBase;
using static ToSic.Eav.Sys.Insights.HtmlHelpers.InsightsHtmlTable;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.Sys;

internal class InsightsLicenses(LazySvc<SystemFingerprint> fingerprint,
    LazySvc<ILicenseService> licenseServiceLazy,
    LazySvc<LicenseCatalog> licenseCatalog) 
    : InsightsProvider(new() { Name = Link, Title = "Licenses Overview" }, connect: [fingerprint, licenseServiceLazy, licenseCatalog])
{
    public static string Link = "Licenses";

    public override string HtmlBody()
    {
        TagBase body = H1("Licenses and Features");

        #region Fingerprints List

        var fpSection =
            +H2("Fingerprints")
            + P("These are the identities as loaded by the system:");

        try
        {
            var fingerprintList = Ol(
                Li(
                    "System Identity: ",
                    Br(),
                    $"Fingerprint: '{fingerprint.Value.GetFingerprint()}'")
            );

            foreach (var entFp in fingerprint.Value.EnterpriseFingerprints)
                fingerprintList.Add(Li(
                    $"Enterprise License: for '{entFp.Title}' {EmojiTrueFalse(entFp.Valid)}",
                    Br(),
                    $"Fingerprint: '{entFp.Fingerprint}' ",
                    "(guid: " + Linker.LinkTo($"{entFp.Guid}", nameof(Entity), KnownAppsConstants.PresetAppId,
                        nameId: entFp.Guid.ToString()) + ")"
                ));

            fpSection += fingerprintList;
        }
        catch
        {
            fpSection += Em("Error creating list of fingerprints");
        }

        #endregion

        #region Licenses with Validity Table

        // Licenses with Validity Table
        var licValiditySection =
            +H2("Licenses")
            + P("These are the licenses as loaded by the system");

        try
        {
            var rows = licenseServiceLazy.Value.All
                .ToList()
                .Select(l => RowFields([
                        EmojiTrueFalse(l.IsEnabled),
                        l.Title,
                        l.LicenseKey,
                        l.Aspect?.Name,
                        l.Aspect?.Guid,
                        EmojiTrueFalse(l.Valid),
                        EmojiTrueFalse(l.SignatureIsValid),
                        EmojiTrueFalse(l.FingerprintIsValid),
                        EmojiTrueFalse(l.VersionIsValid),
                        EmojiTrueFalse(l.ExpirationIsValid),
                        l.Expiration.ToString("yyyy-MM-dd")
                    ]).ToString()
                )
                .ToList();

            licValiditySection += Table().Id("table").Wrap(
                HeadFieldsLeft([
                    "Enabled", "Title", "License Key on this System", "License Name", "License Guid Identifier",
                    "Valid", "VSig", "VFP", "VVer", "VExp",
                    "Expires"
                ]),
                Tbody(rows)
            );
        }
        catch
        {
            licValiditySection += Em("Error creating this section");
        }

        #endregion

        #region Licenses List

        var licFilesSection = H2("License Definitions") as TagBase;

        try
        {
            var licDefinitions = licenseCatalog.Value.List.OrderBy(l => l.Priority);

            var licRows = licDefinitions.Select(l => RowFields([
                new SpecialField(l.Name, tooltip: $"{l.NameId} ({l.Guid})"),
                l.Description,
                l.Priority,
                l.AutoEnable,
                l.FeatureLicense ? "Feature license" : "Built-In"
            ]));

            licFilesSection += Table().Id("table-licenses").Wrap(
                HeadFieldsLeft([SpecialField.Left("Name"), SpecialField.Left("Description"), "Priority", "Auto-Enable", "Special"]),
                Tbody(licRows)
            );
        }
        catch
        {
            licFilesSection += Em("Error creating this section");
        }


        #endregion

        #region Features

        var featsSection = H2("Features")
                           + P("Todo ... ...");

        #endregion

        body += fpSection
                + Hr()
                + licValiditySection
                + Hr()
                + licFilesSection
                + Hr()
                + featsSection;

        return body.ToString()
               + "\n\n"
               + InsightsHtmlParts.JsTableSort();
    }
}