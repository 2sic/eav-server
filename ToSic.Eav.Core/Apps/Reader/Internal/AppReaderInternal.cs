using System.Collections.Immutable;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Internal;

public static class AppReaderInternal
{
    public static IImmutableList<IEntity> GetListPublished(this IAppReader reader) => ((AppState)((AppReader)reader).AppState).ListPublished.List;

    public static IImmutableList<IEntity> GetListNotHavingDrafts(this IAppReader reader) => ((AppState)((AppReader)reader).AppState).ListNotHavingDrafts.List;

    public static AppRelationshipManager GetRelationships(this IAppReader reader) => ((AppState)((AppReader)reader).AppState).Relationships;

    public static IAppStateCache GetCache(this IAppReader reader) => ((AppReader)reader).AppState;
}