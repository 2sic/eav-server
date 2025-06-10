namespace ToSic.Eav.Data.Entities.Sys;

partial record Entity
{
    /// <inheritdoc />
    public IEnumerable<IEntity> Children(string field = null, string type = null)
    {
        var list = Relationships
            .FindChildren(field, type)
            .ToListOpt();
        return list;
    }

    /// <inheritdoc />
    public IEnumerable<IEntity> Parents(string type = null, string field = null)
    {
        var list = Relationships
            .FindParents(type, field)
            .ToListOpt();
        return list;

    }
}