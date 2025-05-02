using ToSic.Eav.Persistence.Versions;

namespace ToSic.Eav.Repository.Efc.Parts;

partial class DbVersioning
{
    /// <summary>
    /// Retrieve the history-list of a specific item
    /// </summary>
    /// <param name="entityId">the Id as used in the DB</param>
    /// <param name="historyId">the id of the item-history</param>
    /// <returns></returns>
    /// <remarks>
    /// Must use entity-id, because even though the Guid feels safer, but it's not unique in this DB, as the same GUID can exist in various apps...
    /// </remarks>
    public ItemHistory GetItem(int entityId, int historyId) => GetItemHistory(entityId, historyId, true).First();


    /// <summary>
    /// Retrieve the history-list of a specific item
    /// </summary>
    /// <param name="entityId">the Id as used in the DB</param>
    /// <param name="includeData">true if the history-data should be included, false it we'll only retrieve the list of records</param>
    /// <returns></returns>
    /// <remarks>
    /// Must use entity-id, because even though the Guid feels safer, but it's not unique in this DB, as the same GUID can exist in various apps...
    /// </remarks>
    public List<ItemHistory> GetHistoryList(int entityId, bool includeData) => GetItemHistory(entityId, 0, includeData);

    /// <summary>
    /// Retrieve the history-list of a specific item
    /// </summary>
    /// <param name="entityId">the Id as used in the DB</param>
    /// <param name="historyId">the optional history-id record - use 0 to get all</param>
    /// <param name="includeData">true if the history-data should be included, false it we'll only retrieve the list of records</param>
    /// <returns></returns>
    /// <remarks>
    /// Must use entity-id, because even though the Guid feels safer, but it's not unique in this DB, as the same GUID can exist in various apps...
    /// </remarks>
    private List<ItemHistory> GetItemHistory(int entityId, int historyId, bool includeData)
    {
        // get Versions from DataTimeline
        var rootQuery = DbContext.SqlDb.ToSicEavDataTimeline
            .Where(t =>
                t.SourceTable == EntitiesTableName
                && t.Operation == Constants.DataTimelineEntityJson
                && t.SourceId == entityId
            );
        if (historyId > 0)
            rootQuery = rootQuery.Where(t => t.SysLogId == historyId);

        var entityVersions = rootQuery
            .OrderByDescending(t => t.SysCreatedDate)
            .Join(DbContext.SqlDb.TsDynDataTransaction, t => t.SysLogId, c => c.TransactionId, (history, log) => new { History = history, Log = log })
            .Select(d =>  new ItemHistory
            {
                TimeStamp = d.History.SysCreatedDate,
                ChangeSetId = d.History.SysLogId.Value,
                HistoryId = d.History.Id,
                User = d.Log.User,
                Json = includeData ? (string.IsNullOrEmpty(d.History.Json) ? _compressor.Value.Decompress(d.History.CJson) : d.History.Json) : null
            })
            .ToList();

        var versionNumber = entityVersions.Count;	// add version number decrement to prevent additional sorting
        foreach (var entityHistoryItem in entityVersions)
            entityHistoryItem.VersionNumber = versionNumber--;

        return entityVersions;
    }

}