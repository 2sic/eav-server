using ToSic.Sys.Capabilities.FeatureSet;

namespace ToSic.Sys.Capabilities.Licenses;

public static class LicensesExtensions
{
    /// <summary>
    /// From a list of licenses containing multiple expirations/activations,
    /// keep only the one with the longest expiration for each license type.
    /// </summary>
    public static IEnumerable<FeatureSetState> DistinctByLongestExpiration(this IEnumerable<FeatureSetState> licenses)
        => licenses
            // must do Distinct = GroupBy + First to ensure we don't have duplicate keys
            .GroupBy(l => l.Aspect)
            .Select(g => g
                // same feature license with longer expiration have priority
                .OrderByDescending(l => l.Expiration)
                .First());
}