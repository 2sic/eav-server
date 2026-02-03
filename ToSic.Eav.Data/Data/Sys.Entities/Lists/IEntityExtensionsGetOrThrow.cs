namespace ToSic.Eav.Data.Sys.Entities;

public static class IEntityExtensionsGetOrThrow
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEntity GetOrThrow(this IEnumerable<IEntity> entities, string? contentType, int id)
        => entities.FindRepoId(id).KeepOrThrowIfInvalid(contentType, id);

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IEntity GetOrThrow(this IEnumerable<IEntity> entities, string? contentType, Guid guid)
        => entities.GetOne(guid).KeepOrThrowIfInvalid(contentType, guid);
}