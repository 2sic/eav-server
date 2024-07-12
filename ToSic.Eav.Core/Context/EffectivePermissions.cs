namespace ToSic.Eav.Context;

/// <summary>
/// Special object which only contains the user permissions.
/// This is important, as various scenarios can result in different permissions,
/// and this object is used to provide them.
///
/// For better understanding: the user itself has these permissions, but sometimes
/// additional information (e.g. an App containing more configuration)
/// can extend the permissions. So a provider would then combine that with the user to get more permissions.
/// </summary>
/// <param name="isContentAdmin"></param>
/// <param name="isSiteAdmin"></param>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EffectivePermissions(bool isSiteAdmin, bool isContentAdmin, bool isContentEditor, bool showDrafts)
{
    public EffectivePermissions(bool all) : this(all, all, all, all) { }

    public EffectivePermissions(bool isSiteAdmin, bool isContentAdmin) : this(isSiteAdmin, isContentAdmin, isContentAdmin, isContentAdmin) { }

    public bool IsContentAdmin { get; } = isContentAdmin;

    public bool IsContentEditor { get; } = isContentEditor;

    public bool IsSiteAdmin { get; } = isSiteAdmin;

    public bool ShowDraftData => showDrafts;
}