using ToSic.Eav.Data;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Apps.State;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class AppStateExtensions
{
    public static IAppStateInternal Internal(this IAppState appState) => appState as IAppStateInternal;

    public static bool IsInherited(this IAppState reader)
        => reader.Internal().StateCache.ParentApp.InheritEntities;  // if it inherits entities, it itself is inherited

    public static bool HasCustomParentApp(this IAppState reader)
    {
        var parentAppGuid = reader?.Internal().ParentAppState?.NameId;
        return !string.IsNullOrEmpty(parentAppGuid) && !AppGuidIsAPreset(parentAppGuid);
    }

    public static bool AppGuidIsAPreset(string parentAppGuid) 
        => parentAppGuid.HasValue() 
           && (parentAppGuid == Constants.PresetName || parentAppGuid == Constants.GlobalPresetName);

    // TODO: @STV - try to use this where possible
    public static bool IsContentApp(this IAppState appState)
        => appState.NameId == Constants.DefaultAppGuid;


    // TODO: @STV - try to use this where possible
    public static bool IsGlobalSettingsApp(this IAppState appState)
        => appState.AppId == Constants.MetaDataAppId;

    // TODO: @STV - try to use this where possible
    public static bool IsSiteSettingsApp(this IAppState appState)
        => appState.NameId == Constants.PrimaryAppGuid;

    public static IEntity GetDraftOrKeep(this IAppState appState, IEntity entity)
        => appState.GetDraft(entity) ?? entity;

    public static IEntity GetDraftOrPublished(this IAppState appState, Guid guid)
        => appState.GetDraftOrKeep(appState.List.One(guid));

}