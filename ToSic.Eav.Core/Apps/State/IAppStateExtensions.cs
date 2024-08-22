using ToSic.Eav.Data;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Apps.State;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class AppStateExtensions
{
    public static bool IsInherited(this IAppReader reader)
        => reader.StateCache.ParentApp.InheritEntities;  // if it inherits entities, it itself is inherited

    public static bool HasCustomParentApp(this IAppReader reader)
    {
        var parentAppGuid = reader?.ParentAppState?.NameId;
        return !string.IsNullOrEmpty(parentAppGuid) && !AppGuidIsAPreset(parentAppGuid);
    }

    public static bool AppGuidIsAPreset(string parentAppGuid) 
        => parentAppGuid.HasValue() 
           && (parentAppGuid == Constants.PresetName || parentAppGuid == Constants.GlobalPresetName);


    // TODO: @STV - try to use this where possible
    public static bool IsGlobalSettingsApp(this IAppIdentity hasAppSpecs)
        => hasAppSpecs.AppId == Constants.MetaDataAppId;


    public static IEntity GetDraftOrKeep(this IAppReader appState, IEntity entity)
        => appState.GetDraft(entity) ?? entity;

    public static IEntity GetDraftOrPublished(this IAppReader appState, Guid guid)
        => appState.GetDraftOrKeep(appState.List.One(guid));

}