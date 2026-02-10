namespace ToSic.Eav.DataSource.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public  interface IDataSourceReset
{
    /// <summary>
    /// Reset the query, so it can be run again. Requires all params to be set again.
    /// </summary>
    [PrivateApi("should be removed soon")]
    void Reset();

}