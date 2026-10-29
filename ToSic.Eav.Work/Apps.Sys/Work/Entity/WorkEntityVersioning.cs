using ToSic.Eav.Apps.Sys.Caching;

using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.Persistence.Versions;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;
using ToSic.Eav.Serialization.Sys;



namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkEntityVersioning(
    AppCachePurger appCachePurger,
    LazySvc<ImportService> import,
    LazySvc<JsonSerializer> jsonSerializer)
    : ServiceWithSetup<IAppWorkContext>("AWk.EntCre", connect: [appCachePurger, jsonSerializer, import])
{

    public List<ItemHistory> VersionHistory(int id, bool includeData = true)
        => MyOptions.NewDbStorage().Versioning.GetHistoryList(id, includeData);

    /// <summary>
    /// Restore an Entity to the specified Version by creating a new Version using the Import
    /// </summary>
    public void VersionRestore(int entityId, int transactionId)
    {
        var db = MyOptions.NewDbStorage();

        // Get Entity in specified Version/TransactionId from DataTimeline using XmlImport
        string? timelineItem;
        try
        {
            timelineItem = db.Versioning.GetItem(entityId, transactionId).Json
                           ?? throw new InvalidOperationException("Json data was null");
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"Error getting EntityId {entityId} with TransactionId {transactionId} from History. {ex.Message}");
        }
        
        var newVersion = jsonSerializer
            .SetInit(j => j.SetApp(MyOptions.AppReader))
            .Value.Deserialize(timelineItem);

        // Restore Entity
        import.SetInit(i => i.Init(MyOptions.ZoneId, MyOptions.AppId, false, false))
            .Value.ImportIntoDb([], new List<Entity> { (Entity)newVersion });

        // Delete Draft (if any)
        var entityDraft = db.Publishing.GetDraftBranchEntityId(entityId);
        if (entityDraft.HasValue)
            db.Entities.DeleteEntities([entityDraft.Value]);

        appCachePurger.Purge(MyOptions.PureIdentity());
    }

}