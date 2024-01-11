namespace ToSic.Eav.DataSource.Internal.Errors;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class ErrorExtensions
{
    public static bool IsError(this IDataSource ds)
    {
        var firstItem = ds.List?.FirstOrDefault();
        return firstItem?.Type?.Name == DataConstants.ErrorTypeName;
    }
}