namespace ToSic.Eav.Data.Entities.Sys.Lists;

public static class IEntityListExtensions
{

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEntity FirstOrDefaultOfType(this IEnumerable<IEntity> list, string typeName)
        => list.FirstOrDefault(e => e.Type.Is(typeName));

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEntity GetOrThrow(this IEnumerable<IEntity> entities, string contentType, int id)
        => entities.FindRepoId(id).KeepOrThrowIfInvalid(contentType, id);

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEntity GetOrThrow(this IEnumerable<IEntity> entities, string contentType, Guid guid)
        => entities.One(guid).KeepOrThrowIfInvalid(contentType, guid);
}