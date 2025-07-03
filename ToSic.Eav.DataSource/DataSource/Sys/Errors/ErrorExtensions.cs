using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.DataSource.Internal.Errors;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ErrorExtensions
{
    public static bool IsError(this IDataSource ds)
        => (ds.List?.FirstOrDefault())?.IsError() == true;

    public static bool IsError(this IEntity entity)
        => entity?.Type?.Name == DataConstants.ErrorTypeName;
}