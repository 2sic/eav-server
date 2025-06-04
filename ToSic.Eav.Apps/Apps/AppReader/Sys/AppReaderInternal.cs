using System.Collections.Immutable;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Apps.Sys;

namespace ToSic.Eav.Apps.AppReader.Sys;

public static class AppReaderInternal
{
    public static IImmutableList<IEntity> GetListPublished(this IAppReader reader)
        => ((AppState)reader.GetCache()).ListPublished.List;

    public static IImmutableList<IEntity> GetListNotHavingDrafts(this IAppReader reader)
        => ((AppState)reader.GetCache()).ListNotHavingDrafts.List;

    internal static AppRelationshipManager GetRelationships(this IAppReader reader)
        => reader.GetCache().Relationships as AppRelationshipManager;

    public static IAppStateCache GetCache(this IAppReader reader)
        => ((AppReader)reader).AppState;

    public static IAppStateCache? GetParentCache(this IAppReader reader)
        => reader.GetCache().ParentApp?.AppState;
}