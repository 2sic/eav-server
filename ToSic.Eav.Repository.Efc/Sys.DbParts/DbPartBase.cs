namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

// Note: Don't connect service - this was initialized before...
internal class DbPartBase(DbStorage.DbStorage dbStore, string logName) : HelperBase(dbStore.Log, logName)
{
    protected DbStorage.DbStorage DbContext { get; } = dbStore;

    /// <summary>
    /// Logger for the details of the deserialization process.
    /// Goal is that it can be enabled/disabled as needed.
    /// </summary>
    internal ILog? LogDetails =>
        DbContext.LogDetails == null ? null : field ??= Log;

    internal ILog? LogSummary =>
        DbContext.LogSummary == null ? null : field ??= Log;

}