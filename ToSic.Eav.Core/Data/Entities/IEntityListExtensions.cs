using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Data;

public static class IEntityListExtensions
{

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IEntity FirstOrDefaultOfType(this IEnumerable<IEntity> list, string typeName)
        => list.FirstOrDefault(e => e.Type.Is(typeName));

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IEntity GetOrThrow(this IEnumerable<IEntity> entities, string contentType, int id)
        => entities.FindRepoId(id).KeepOrThrowIfInvalid(contentType, id);

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IEntity GetOrThrow(this IEnumerable<IEntity> entities, string contentType, Guid guid)
        => entities.One(guid).KeepOrThrowIfInvalid(contentType, guid);
}