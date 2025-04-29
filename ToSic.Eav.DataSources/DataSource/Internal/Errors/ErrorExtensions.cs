namespace ToSic.Eav.DataSource.Internal.Errors;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class ErrorExtensions
{
    public static bool IsError(this IDataSource ds)
        => (ds.List?.FirstOrDefault()).IsError();

    public static bool IsError(this IEntity entity)
        => entity?.Type?.Name == DataConstants.ErrorTypeName;
}