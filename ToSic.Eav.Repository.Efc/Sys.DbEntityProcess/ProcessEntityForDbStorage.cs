using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;

/// <summary>
/// Experimental
///
/// Trying to extract logic steps for an entity to better reorganize sequencing.
/// </summary>
internal class ProcessEntityForDbStorage(DbStorage.DbStorage dbStorage, DataBuilder builder) : ServiceBase("DB.PrepEy")
{

    #region Helpers & Setup

    public EntityProcessServices Services { get; set; } = null!;

    public void Start(ICollection<IEntityPair<SaveOptions>> entityOptionPairs)
    {
        Services = new(dbStorage, builder);
        Services.Start(entityOptionPairs);
    }


    #endregion

    public EntityProcessData PreProcess(IEntityPair<SaveOptions> entityOptionPair, bool logDetails)
    {
        // 1. Prepare data
        var data = new EntityProcessData
        {
            NewEntity = entityOptionPair.Entity,
            Options = entityOptionPair.Partner,
            LogDetails = logDetails,
            Progress = 0,
        };

        foreach (var process in GetStandardProcess())
        {
            data = process.Process(Services, data.NextStep());
            if (data.Exception != null)
                throw data.Exception;
        }

        return data;
    }

    private List<IEntityProcess> GetStandardProcess() =>
    [
        new Process1Preflight(),
        new Process2PublishAndContentType(),
        
        new Process3New1LastChecks(),
        new Process3New2DbStoreHeader(),
        new Process3New3DbStoreJson(),

        new Process3Upd1DbPreload(),
        new Process3Upd2PrepareUpdate(),
        new Process3Upd3ClearValues(),

        new Process4TableValues(),
        new Process4JsonValues(),

        new Process5TableRelationships(),

        new Process6Versioning(),
    ];

    //public EntityProcessData Process1Preflight(EntityProcessData data)
    //{
    //    var newEnt = data.NewEntity;
        
    //    var l = LogDetails.Fn<EntityProcessData>($"id:{newEnt?.EntityId}/{newEnt?.EntityGuid}, logDetails:{data.LogDetails}");

    //    if (newEnt == null)
    //        return l.ReturnAsError(data with { Exception = new ArgumentNullException(nameof(newEnt)) });

    //    if (newEnt.Type == null! /* paranoid */)
    //        return l.ReturnAsError(data with { Exception = new("trying to save entity without known content-type, cannot continue") });

    //    #region Test what languages are given, and check if they exist in the target system

    //    // continue here - must ensure that the languages are passed in, cached - or are cached on the DbEntity... for multiple saves
    //    var zoneLangs = data.Options.Languages ?? throw new("languages missing in save-options. cannot continue");

    //    var usedLanguages = data.NewEntity.GetUsedLanguages();
    //    if (usedLanguages.Count > 0)
    //        if (!usedLanguages.All(lang => zoneLangs.Any(zl => zl.Matches(lang.Key))))
    //        {
    //            var langList = l.Try(() => string.Join(",", usedLanguages.Select(lang => lang.Key)));
    //            return l.ReturnAsError(data with
    //            {
    //                    Exception = new($"entity has languages missing in zone - entity: {usedLanguages.Count} zone: {zoneLangs.Count} used-list: '{langList}'")
    //                }
    //            );
    //        }

    //    if (data.LogDetails)
    //    {
    //        l.A($"lang checks - zone language⋮{zoneLangs.Count}, usedLanguages⋮{usedLanguages.Count}");
    //        var zoneLangList = l.Try(() => string.Join(",", zoneLangs.Select(z => z.EnvironmentKey)));
    //        var usedLangList = l.Try(() => string.Join(",", usedLanguages.Select(u => u.Key)));
    //        l.A($"langs zone:[{zoneLangList}] used:[{usedLangList}]");
    //    }


    //    #endregion Test languages exist

    //    // check if saving should be with db-type or with the plain json
    //    data = data with { SaveJson = newEnt.UseJson() };
    //    if (data.LogDetails)
    //        l.A($"save json:{data.SaveJson}");

    //    return l.Return(data);
    //}

    //public EntityProcessData Process2PublishAndContentType(EntityProcessData data)
    //{
    //    var l = LogDetails.Fn<EntityProcessData>();

    //    #region Step 2: check header record - does it already exist, what ID should we use, etc.

    //    // If we think we'll update an existing entity...
    //    // ...we have to check if we'll actually update the draft of the entity
    //    // ...or create a new draft (branch)
    //    var (existingDraftId, hasAdditionalDraft, entity) = PublishingAnalyzer.GetDraftAndCorrectIdAndBranching(data.NewEntity, data.Options, data.LogDetails); // TODO: later just pass in data
    //    data = data with { NewEntity = entity, }; // may have been replaced with an updated IEntity during corrections

    //    var (contentTypeId, attributeDefs) = StructureAnalyzer.GetContentTypeAndAttribIds(data.SaveJson, data.NewEntity, data.LogDetails);

    //    data = data with
    //    {
    //        IsNew = data.NewEntity.EntityId <= 0, // remember how we want to work...
    //        ExistingDraftId = existingDraftId,
    //        HasAdditionalDraft = hasAdditionalDraft,
    //        ContentTypeId = contentTypeId,
    //        AttributeDefs = attributeDefs,
    //    };

    //    if (data.LogDetails)
    //        l.A($"entity id:{data.NewEntity.EntityId} - will treat as new:{data.IsNew}");

    //    #endregion Step 2

    //    return l.Return(data);
    //}

    //public EntityProcessData Process3New1LastChecks(EntityProcessData data)
    //{
    //    var l = LogDetails.Fn<EntityProcessData>();

    //    if (!data.IsNew)
    //        return l.Return(data, "not new, not my job");

    //    if (data.NewEntity.EntityGuid == Guid.Empty)
    //    {
    //        if (data.LogDetails)
    //            l.A("New entity guid was null, will throw exception");
    //        return l.Return(data with
    //        {
    //            Exception = new ArgumentException("can't create entity in DB with guid null - entities must be fully prepared before sending to save"),
    //        });
    //    }

    //    return l.Return(data);
    //}

    //public EntityProcessData Process3New2DbStoreHeader(EntityProcessData data)
    //{
    //    var l = LogDetails.Fn<EntityProcessData>();

    //    if (!data.IsNew)
    //        return l.Return(data, "not new, skip");

    //    data = data with { DbEntity = DbEntity.CreateDbRecord(data.NewEntity, TransactionId, data.ContentTypeId) };

    //    // update the ID - for versioning and/or json persistence
    //    data = data with { NewEntity = builder.Entity.CreateFrom(data.NewEntity, id: data.DbEntity.EntityId) };

    //    // prepare export for save json OR versioning later on
    //    data = data with { JsonExport = DbEntity.GenerateJsonOrReportWhyNot(data.NewEntity, data.LogDetails) };

    //    if (data.SaveJson)
    //    {
    //        var l3 = l.Fn($"id:{data.NewEntity.EntityId}, guid:{data.NewEntity.EntityGuid}");
    //        data.DbEntity.Json = data.JsonExport;
    //        data.DbEntity.ContentType = data.NewEntity.Type.NameId;
    //        dbStorage.DoAndSaveWithoutChangeDetection(() => dbStorage.SqlDb.Update(data.DbEntity),
    //            "update json");
    //        l3.Done();
    //    }
    //    l.Done($"i:{data.DbEntity.EntityId}, guid:{data.DbEntity.EntityGuid}");

    //    return l.Return(data);
    //}

    //public EntityProcessData Process3Upd1DbPreload(EntityProcessData data)
    //{
    //    var l = LogDetails.Fn<EntityProcessData>();

    //    if (data.IsNew)
    //        return l.Return(data, "new, skip");

    //    // get the published one (entityId is always the published id)
    //    var dbEntity = dbStorage.Entities.GetDbEntityFull(data.NewEntity.EntityId);

    //    // new: always change the draft if there is one! - it will then either get published, or not...
    //    data = data with
    //    {
    //        DbEntity = dbEntity,
    //        StateChanged = dbEntity.IsPublished != data.NewEntity.IsPublished
    //    };

    //    return l.Return(data);
    //}


    //public EntityProcessData Process99Template(EntityProcessData data)
    //{
    //    var l = LogDetails.Fn<EntityProcessData>();

    //    return l.Return(data);
    //}
}