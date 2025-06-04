using ToSic.Eav.Data.Entities.Sys;
using ToSic.Eav.ImportExport.Internal;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.Persistence.Versions;
using ToSic.Eav.Serialization.Sys;
using IEntity = ToSic.Eav.Data.IEntity;


namespace ToSic.Eav.Apps.Internal.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkEntityVersioning : WorkUnitBase<IAppWorkCtxWithDb>
{
    private readonly LazySvc<ImportService> _import;
    public AppCachePurger AppCachePurger { get; }
    private readonly LazySvc<JsonSerializer> _jsonSerializer;

    public WorkEntityVersioning(AppCachePurger appCachePurger, LazySvc<ImportService> import, LazySvc<JsonSerializer> jsonSerializer)
        : base("AWk.EntCre", connect: [appCachePurger, jsonSerializer, import])
    {
        AppCachePurger = appCachePurger;
        _jsonSerializer = jsonSerializer.SetInit(j => j.SetApp(AppWorkCtx.AppReader));
        _import = import.SetInit(i => i.Init(AppWorkCtx.ZoneId, AppWorkCtx.AppId, false, false));
    }


    public List<ItemHistory> VersionHistory(int id, bool includeData = true) => AppWorkCtx.DataController.Versioning.GetHistoryList(id, includeData);

    /// <summary>
    /// Restore an Entity to the specified Version by creating a new Version using the Import
    /// </summary>
    public void VersionRestore(int entityId, int transactionId)
    {
        // Get Entity in specified Version/TransactionId
        var newVersion = PrepareRestoreEntity(entityId, transactionId);

        // Restore Entity
        _import.Value.ImportIntoDb(null, new List<Entity> { newVersion as Entity });

        // Delete Draft (if any)
        var entityDraft = AppWorkCtx.DataController.Publishing.GetDraftBranchEntityId(entityId);
        if (entityDraft.HasValue)
            AppWorkCtx.DataController.Entities.DeleteEntity(entityDraft.Value);

        AppCachePurger.Purge(AppWorkCtx);
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
            var timelineItem = AppWorkCtx.DataController.Versioning.GetItem(entityId, transactionId).Json;
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