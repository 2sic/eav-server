using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Specs;
using ToSic.Eav.Data;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Apps.State;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class AppStateExtensions
{
    public static IAppReader Internal(this IAppState appState) => appState as IAppReader;

    public static bool IsInherited(this IAppSpecsWithStateAndCache reader)
        => reader.Cache.ParentApp.InheritEntities;  // if it inherits entities, it itself is inherited

    public static bool HasCustomParentApp(this IAppReader reader)
    {
        var parentAppGuid = reader?.ParentAppState?.NameId;
        return !string.IsNullOrEmpty(parentAppGuid) && !AppGuidIsAPreset(parentAppGuid);
    }

    public static bool AppGuidIsAPreset(string parentAppGuid) 
        => parentAppGuid.HasValue() 
           && (parentAppGuid == Constants.PresetName || parentAppGuid == Constants.GlobalPresetName);

    // TODO: @STV - try to use this where possible
    public static bool IsContentApp(this IHas<IAppSpecs> hasAppSpecs)
        => hasAppSpecs.Value.NameId == Constants.DefaultAppGuid;


    // TODO: @STV - try to use this where possible
    public static bool IsGlobalSettingsApp(this IHas<IAppSpecs> hasAppSpecs)
        => hasAppSpecs.Value.AppId == Constants.MetaDataAppId;

    // TODO: @STV - try to use this where possible
    public static bool IsSiteSettingsApp(this IHas<IAppSpecs> hasAppSpecs)
        => hasAppSpecs.Value.NameId == Constants.PrimaryAppGuid;

    public static IEntity GetDraftOrKeep(this IAppState appState, IEntity entity)
        => appState.GetDraft(entity) ?? entity;

    public static IEntity GetDraftOrPublished(this IAppState appState, Guid guid)
        => appState.GetDraftOrKeep(appState.List.One(guid));

}