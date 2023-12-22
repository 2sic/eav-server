using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Data;

partial class Entity
{
    /// <inheritdoc />
    public List<IEntity> Children(string field = null, string type = null)
    {
        var list = Relationships
            .FindChildren(field, type)
            .ToList();
        return list;
    }

    /// <inheritdoc />
    public List<IEntity> Parents(string type = null, string field = null)
    {
        var list = Relationships
            .FindParents(type, field)
            .ToList();
        return list;

    }
}