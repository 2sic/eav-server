using System.Collections;
using System.Linq;

namespace ToSic.Eav.Data.PropertyLookup;

public static class PropReqResultExtensions
{
    public static IEntity GetFirstResultEntity(this PropReqResult resultSet)
    {
        if (!(resultSet?.Result is IEnumerable enumResult)) return null;
        var x = enumResult.Cast<object>();
        return x.FirstOrDefault() is ICanBeEntity canBeEntity 
            ? canBeEntity.Entity 
            : null;
    }
}