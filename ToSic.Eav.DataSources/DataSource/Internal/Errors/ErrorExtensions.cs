namespace ToSic.Eav.DataSource.Internal.Errors;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class ErrorExtensions
{
    public static bool IsError(this IDataSource ds)
        => (ds.List?.FirstOrDefault()).IsError();

    public static bool IsError(this IEntity entity)
        => entity?.Type?.Name == DataConstants.ErrorTypeName;
}