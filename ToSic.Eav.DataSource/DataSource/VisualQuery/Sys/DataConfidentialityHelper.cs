using ToSic.Sys.Users;

namespace ToSic.Eav.DataSource.VisualQuery.Sys;

internal static class DataConfidentialityHelper
{
    public static bool IsAllowed(this DataSourceInfo? dsi, IUser? user)
    {
        // If no info is available, return false
        if (dsi?.VisualQuery == null || user == null)
            return false;

        // superusers always allowed
        return dsi.VisualQuery.DataConfidentiality.IsAllowed(user);
    }

    /// <summary>
    /// Check if a user is allowed to this confidentiality level.
    /// Note that it assumes a correctly built user object, e.g. not null.
    /// ...and having correct properties set - so IsSiteAdmin is expected to be true, if IsSystemAdmin is true, etc.
    /// </summary>
    /// <param name="confidentiality"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public static bool IsAllowed(this DataConfidentiality confidentiality, IUser user) =>
        confidentiality switch
        {
            // Never should never be allowed
            DataConfidentiality.Never => false,
            // Unknown is never allowed, for safety
            DataConfidentiality.Unknown => user.IsSystemAdmin,
            // Public is always allowed
            DataConfidentiality.Public => true,
            // System & Secret requires system admin - already handled above, but keeping for clarity
            DataConfidentiality.System => user.IsSystemAdmin,
            DataConfidentiality.Secret => user.IsSystemAdmin,
            // Confidential requires at least site admin
            DataConfidentiality.Confidential => user.IsSiteAdmin,
            // Internal requires at least content editor
            DataConfidentiality.Internal => user.IsContentEditor,
            _ => false
        };
}
