using System.Linq;
using ToSic.Eav.WebApi.Sys.Insights;
using ToSic.Razor.Markup;
using static ToSic.Eav.WebApi.Sys.Insights.InsightsHtmlBase;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys
{
    public partial class InsightsControllerReal
    {
        private string Licenses()
        {
            var intro = H1("Licenses and Features") as TagBase;

            // Fingerprint
            var fingerprintList = Ol(
                Li("Built in Fingerprint: " + _fingerprint.Value.GetFingerprint())
            );

            foreach (var entFp in _fingerprint.Value.EnterpriseFingerprintsWIP)
                fingerprintList.Add(Li(
                    $"Enterprise: '{entFp.Title}' {EmojiTrueFalse(entFp.Valid)} - '{entFp.Fingerprint}', '{entFp.Guid}'")
                );

            intro = intro
                      + H2("Fingerprints")
                      + P("These are the fingerprints as loaded by the system")
                      + fingerprintList;


            // Licenses
            intro = intro
                    + H2("Licenses")
                    + P("These are the licenses as loaded by the system");

            var rows = _licenseServiceLazy.Value.All
                .ToList()
                .Select(l => InsightsHtmlTable.RowFields(
                        EmojiTrueFalse(l.Enabled),
                        l.Title,
                        l.LicenseKey,
                        l.License?.Name,
                        l.License?.Guid,
                        EmojiTrueFalse(l.Valid),
                        EmojiTrueFalse(l.SignatureIsValid),
                        EmojiTrueFalse(l.FingerprintIsValid),
                        EmojiTrueFalse(l.VersionIsValid),
                        EmojiTrueFalse(l.ExpirationIsValid),
                        l.Expiration.ToString("yyyy-MM-dd")
                    ).ToString()
                )
                .ToList();

            var msg = intro
                      + Table().Id("table").Wrap(
                          InsightsHtmlTable.HeadFields(
                              "Enabled", "Title", "License Key on this System", "License Name", "License Guid Identifier", 
                              "Valid", "VSig", "VFP", "VVer", "VExp",
                              "Expires"
                          ),
                          Tbody(rows)
                      );

            msg = msg + Hr() + H2("Features");

            msg = msg + P("Todo...");

            return msg.ToString();
        }
    }
}
