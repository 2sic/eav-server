﻿using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys.State;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class AppStateExtensions
{
    public static bool IsInherited(this IAppReader reader)
        => ((ParentAppState)reader.GetCache().ParentApp).InheritEntities;  // if it inherits entities, it itself is inherited

    public static bool HasCustomParentApp(this IAppReader reader)
    {
        var parentAppGuid = reader?.GetParentCache()?.NameId;
        return !string.IsNullOrEmpty(parentAppGuid) && !AppGuidIsAPreset(parentAppGuid!);
    }

    public static bool AppGuidIsAPreset(string parentAppGuid)
        => parentAppGuid.HasValue()
           && (parentAppGuid == KnownAppsConstants.PresetName || parentAppGuid == KnownAppsConstants.GlobalPresetName);


    // TODO: @STV - try to use this where possible
    public static bool IsGlobalSettingsApp(this IAppIdentity hasAppSpecs)
        => hasAppSpecs.AppId == KnownAppsConstants.MetaDataAppId;


    [return: NotNullIfNotNull(nameof(entity))]
    public static IEntity? GetDraftOrKeep(this IAppReader appReader, IEntity? entity)
        => appReader.GetDraft(entity) ?? entity;

    public static IEntity? GetDraftOrPublished(this IAppReader appReader, Guid guid)
        => appReader.GetDraftOrKeep(appReader.List.One(guid));

}