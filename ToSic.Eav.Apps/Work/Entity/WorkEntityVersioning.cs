using System;
using System.Collections.Generic;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Serialization;
using ToSic.Eav.Persistence.Versions;
using ToSic.Lib.DI;
using IEntity = ToSic.Eav.Data.IEntity;


namespace ToSic.Eav.Apps.Work;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class WorkEntityVersioning : WorkUnitBase<IAppWorkCtxWithDb>
{
    private readonly LazySvc<ImportService> _import;
    public AppCachePurger AppCachePurger { get; }
    private readonly LazySvc<JsonSerializer> _jsonSerializer;

    public WorkEntityVersioning(AppCachePurger appCachePurger, LazySvc<ImportService> import, LazySvc<JsonSerializer> jsonSerializer) : base("AWk.EntCre")
    {
        ConnectServices(
            AppCachePurger = appCachePurger,
            _jsonSerializer = jsonSerializer.SetInit(j => j.SetApp(AppWorkCtx.AppState)),
            _import = import.SetInit(i => i.Init(AppWorkCtx.ZoneId, AppWorkCtx.AppId, false, false))

        );
    }


    public List<ItemHistory> VersionHistory(int id, bool includeData = true) => AppWorkCtx.DataController.Versioning.GetHistoryList(id, includeData);

    /// <summary>
    /// Restore an Entity to the specified Version by creating a new Version using the Import
    /// </summary>
    public void VersionRestore(int entityId, int changeId)
    {
        // Get Entity in specified Version/ChangeId
        var newVersion = PrepareRestoreEntity(entityId, changeId);

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
    /// <param name="changeId">ChangeId to retrieve</param>
    ///// <param name="defaultCultureDimension">Default Language</param>
    private IEntity PrepareRestoreEntity(int entityId, int changeId)
    {
        //var deserializer = Parent.ServiceProvider.Build<JsonSerializer>().Init(Parent.AppState, Log);

        var str = GetFromTimelime(entityId, changeId);
        return _jsonSerializer.Value.Deserialize(str);

    }

    private string GetFromTimelime(int entityId, int changeId)
    {
        try
        {
            var timelineItem = AppWorkCtx.DataController.Versioning.GetItem(entityId, changeId).Json;
            if (timelineItem != null) return timelineItem;
            throw new InvalidOperationException(
                $"EntityId {entityId} with ChangeId {changeId} not found in DataTimeline.");
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException(
                $"Error getting EntityId {entityId} with ChangeId {changeId} from DataTimeline. {ex.Message}");
        }
    }

}