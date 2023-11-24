using System.Linq;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSource;

public static class ErrorExtensions
{
    public static bool IsError(this IDataSource ds)
    {
        var firstItem = ds.List?.FirstOrDefault();
        return firstItem?.Type?.Name == DataConstants.ErrorTypeName;
    }
}