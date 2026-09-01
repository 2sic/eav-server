using ToSic.Eav.Apps.Sys.Caching;

using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.Persistence.Versions;
using ToSic.Eav.Serialization.Sys;



namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkEntityVersioning : ServiceWithSetup<IAppWorkContext>
{
    private readonly LazySvc<ImportService> _import;
    public AppCachePurger AppCachePurger { get; }
    private readonly LazySvc<JsonSerializer> _jsonSerializer;

    public WorkEntityVersioning(AppCachePurger appCachePurger, LazySvc<ImportService> import, LazySvc<JsonSerializer> jsonSerializer)
        : base("AWk.EntCre", connect: [appCachePurger, jsonSerializer, import])
    {
        AppCachePurger = appCachePurger;
        _jsonSerializer = jsonSerializer.SetInit(j => j.SetApp(MyOptions.AppReader));
        _import = import.SetInit(i => i.Init(MyOptions.ZoneId, MyOptions.AppId, false, false));
    }


    public List<ItemHistory> VersionHistory(int id, bool includeData = true) => MyOptions.DbStorage.Versioning.GetHistoryList(id, includeData);

    /// <summary>
    /// Restore an Entity to the specified Version by creating a new Version using the Import
    /// </summary>
    public void VersionRestore(int entityId, int transactionId)
    {
        // Get Entity in specified Version/TransactionId
        var newVersion = PrepareRestoreEntity(entityId, transactionId);

        // Restore Entity
        _import.Value.ImportIntoDb([], new List<Entity> { (Entity)newVersion });

        // Delete Draft (if any)
        var entityDraft = MyOptions.DbStorage.Publishing.GetDraftBranchEntityId(entityId);
        if (entityDraft.HasValue)
            MyOptions.DbStorage.Entities.DeleteEntities([entityDraft.Value]);

        AppCachePurger.Purge(MyOptions.PureIdentity());
    }


    /// <summary>
    /// Get an Entity in the specified Version from DataTimeline using XmlImport
    /// </summary>
    /// <param name="entityId">EntityId</param>
    /// <param name="transactionId">TransactionId to retrieve</param>
    ///// <param name="defaultCultureDimension">Default Language</param>
    private IEntity PrepareRestoreEntity(int entityId, int transactionId)
    {
        //var deserializer = Parent.ServiceProvider.Build<JsonSerializer>().Init(Parent.AppState, Log);

        var str = GetFromHistory(entityId, transactionId);
        return _jsonSerializer.Value.Deserialize(str);

    }

    private string GetFromHistory(int entityId, int transactionId)
    {
        try
        {
            var timelineItem = MyOptions.DbStorage.Versioning.GetItem(entityId, transactionId).Json;
            if (timelineItem != null) return timelineItem;
            throw new InvalidOperationException(
                $"EntityId {entityId} with TransactionId {transactionId} not found in History.");
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(
                $"Error getting EntityId {entityId} with TransactionId {transactionId} from History. {ex.Message}");
        }
    }

}