using System;
using ToSic.Eav.Data;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Apps;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class AppStateExtensions
{

    public static bool IsInherited(this AppState appState)
        => appState.ParentApp.InheritEntities;  // if it inherits entities, it itself is inherited

    public static bool IsInherited(this IAppState reader)
        => reader.Internal().AppState.ParentApp.InheritEntities;  // if it inherits entities, it itself is inherited

    public static bool HasCustomParentApp(this IAppState reader) => reader.Internal().AppState.HasCustomParentApp();

    public static bool HasCustomParentApp(this AppState states)
    {
        var parentAppGuid = states?.ParentApp?.AppState?.NameId;
        return !string.IsNullOrEmpty(parentAppGuid) && !AppGuidIsAPreset(parentAppGuid);
    }

    public static bool AppGuidIsAPreset(string parentAppGuid) 
        => parentAppGuid.HasValue() 
           && (parentAppGuid == Constants.PresetName || parentAppGuid == Constants.GlobalPresetName);

    // TODO: @STV - try to use this where possible
    public static bool IsContentApp(this AppState appState)
        => appState.NameId == Constants.DefaultAppGuid;

    // TODO: @STV - try to use this where possible
    public static bool IsGlobalSettingsApp(this AppState appState)
        => appState.AppId == Constants.MetaDataAppId;

    // TODO: @STV - try to use this where possible
    public static bool IsSiteSettingsApp(this AppState appState)
        => appState.NameId == Constants.PrimaryAppGuid;

    public static IEntity GetDraftOrKeep(this AppState appState, IEntity entity) => appState.GetDraft(entity) ?? entity;
    public static IEntity GetDraftOrPublished(this AppState appState, Guid guid) => appState.GetDraftOrKeep(appState.List.One(guid));
}