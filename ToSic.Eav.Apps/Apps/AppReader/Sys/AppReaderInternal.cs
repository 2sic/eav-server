using System.Collections.Immutable;
using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Apps.Sys.State.Managers;

namespace ToSic.Eav.Apps.AppReader.Sys;

public static class AppReaderInternal
{
    public static IImmutableList<IEntity> GetListPublished(this IAppReader reader)
        => ((AppState)reader.GetCache()).Publishing.ListPublished.List;

    public static IImmutableList<IEntity> GetListNotHavingDrafts(this IAppReader reader)
        => ((AppState)reader.GetCache()).Publishing.ListNotHavingDrafts.List;

    internal static AppRelationshipManager GetRelationships(this IAppReader reader)
        => (AppRelationshipManager)reader.GetCache().Relationships;

    public static IAppStateCache GetCache(this IAppReader reader)
        => ((AppReader)reader).AppState;

    public static IAppStateCache? GetParentCache(this IAppReader reader)
        => reader.GetCache().ParentApp.AppState;
}