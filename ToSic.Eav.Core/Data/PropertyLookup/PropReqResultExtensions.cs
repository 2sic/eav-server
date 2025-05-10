using System.Collections;

namespace ToSic.Eav.Data.PropertyLookup;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class PropReqResultExtensions
{
    public static IEntity GetFirstResultEntity(this PropReqResult resultSet)
    {
        if (resultSet?.Result is not IEnumerable enumResult) return null;
        var x = enumResult.Cast<object>();
        return x.FirstOrDefault() is ICanBeEntity canBeEntity 
            ? canBeEntity.Entity 
            : null;
    }
}