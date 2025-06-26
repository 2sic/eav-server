namespace ToSic.Sys.Users.Permissions;

/// <summary>
/// Special object which only contains the user permissions.
/// This is important, as various scenarios can result in different permissions,
/// and this object is used to provide them.
///
/// For better understanding: the user itself has these permissions, but sometimes
/// additional information (e.g. an App containing more configuration)
/// can extend the permissions. So a provider would then combine that with the user to get more permissions.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public record EffectivePermissions(bool IsSiteAdmin, bool IsContentAdmin, bool IsContentEditor, bool ShowDraftData)
{
    public EffectivePermissions(bool all)
        : this(all, all, all, all)
    { }

    public EffectivePermissions(bool isSiteAdmin, bool isContentAdmin)
        : this(isSiteAdmin, isContentAdmin, isContentAdmin, isContentAdmin)
    { }

}