using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

partial class DbEntity
{
    private int SaveEntity(IEntityPair<SaveOptions> entityOptionPair, bool logDetails)
    {
        var newEnt = entityOptionPair.Entity;
        var so = entityOptionPair.Partner;
        var l = LogDetails.Fn<int>($"id:{newEnt?.EntityId}/{newEnt?.EntityGuid}, logDetails:{logDetails}");

        #region Step 1: Do some initial error checking and preparations

        if (newEnt == null)
            throw new ArgumentNullException(nameof(newEnt));

        if (newEnt.Type == null)
            throw new("trying to save entity without known content-type, cannot continue");

        #region Test what languages are given, and check if they exist in the target system

        // continue here - must ensure that the languages are passed in, cached - or are cached on the DbEntity... for multiple saves
        var zoneLangs = so.Languages ?? throw new("languages missing in save-options. cannot continue");

        var usedLanguages = newEnt.GetUsedLanguages();
        if (usedLanguages.Count > 0)
            if (!usedLanguages.All(lang => zoneLangs.Any(zl => zl.Matches(lang.Key))))
            {
                var langList = l.Try(() => string.Join(",", usedLanguages.Select(lang => lang.Key)));
                throw new($"entity has languages missing in zone - entity: {usedLanguages.Count} zone: {zoneLangs.Count} used-list: '{langList}'");
            }

        if (logDetails)
        {
            l.A($"lang checks - zone language⋮{zoneLangs.Count}, usedLanguages⋮{usedLanguages.Count}");
            var zoneLangList = l.Try(() => string.Join(",", zoneLangs.Select(z => z.EnvironmentKey)));
            var usedLangList = l.Try(() => string.Join(",", usedLanguages.Select(u => u.Key)));
            l.A($"langs zone:[{zoneLangList}] used:[{usedLangList}]");
        }


        #endregion Test languages exist

        // check if saving should be with db-type or with the plain json
        var saveJson = newEnt.UseJson();
        if (logDetails)
            l.A($"save json:{saveJson}");

        #endregion Step 1


        #region Step 2: check header record - does it already exist, what ID should we use, etc.

        // If we think we'll update an existing entity...
        // ...we have to check if we'll actually update the draft of the entity
        // ...or create a new draft (branch)
        var (existingDraftId, hasAdditionalDraft, entity) = Preprocessor.Services.PublishingAnalyzer.GetDraftAndCorrectIdAndBranching(newEnt, so, logDetails);
        newEnt = entity; // may have been replaced with an updated IEntity during corrections

        var isNew = newEnt.EntityId <= 0; // remember how we want to work...
        if (logDetails)
            l.A($"entity id:{newEnt.EntityId} - will treat as new:{isNew}");

        var (contentTypeId, attributeDefs) = Preprocessor.Services.StructureAnalyzer.GetContentTypeAndAttribIds(saveJson, newEnt, logDetails);

        #endregion Step 2


        var entityId = 0;
        TsDynDataEntity? dbEnt;
        string? jsonExport = null;

        var transactionId = DbContext.Versioning.GetTransactionId();

        DbContext.DoInTransaction(() =>
        {
            #region Step 3: either create a new entity, or if it's an update, do draft/published checks to ensure correct data

            // is New vs. Update
            if (isNew)
            {
                var l2 = l.Fn("Create new", timer: true);
                if (newEnt.EntityGuid == Guid.Empty)
                {
                    if (logDetails)
                        l2.A("New entity guid was null, will throw exception");
                    throw new ArgumentException("can't create entity in DB with guid null - entities must be fully prepared before sending to save");
                }

                dbEnt = CreateDbRecord(newEnt, transactionId, contentTypeId);
                // update the ID - for versioning and/or json persistence
                newEnt = builder.Entity.CreateFrom(newEnt, id: dbEnt.EntityId);

                // prepare export for save json OR versioning later on
                jsonExport = GenerateJsonOrReportWhyNot(newEnt, logDetails);

                if (saveJson)
                {
                    var l3 = l2.Fn($"id:{newEnt.EntityId}, guid:{newEnt.EntityGuid}");
                    dbEnt.Json = jsonExport;
                    dbEnt.ContentType = newEnt.Type.NameId;
                    DbContext.DoAndSaveWithoutChangeDetection(() => DbContext.SqlDb.Update(dbEnt),
                        "update json");
                    l3.Done();
                }
                l2.Done($"i:{dbEnt.EntityId}, guid:{dbEnt.EntityGuid}");
            }
            // is Update
            else
            {
                #region Step 3b: Check published (only if not new) - make sure we don't have multiple drafts

                // new: always change the draft if there is one! - it will then either get published, or not...
                dbEnt = DbContext.Entities
                    .GetDbEntityFull(newEnt.EntityId); // get the published one (entityId is always the published id)

                var stateChanged = dbEnt.IsPublished != newEnt.IsPublished;
                var paramsMsg =
                    $"used existing i:{dbEnt.EntityId}, guid:{dbEnt.EntityGuid}, newState:{newEnt.IsPublished}, state-changed:{stateChanged}, has-additional-draft:{hasAdditionalDraft}";
                l.Do(paramsMsg, () =>
                {

                    #region If draft but should be published, correct what's necessary

                    // Update as Published but Current Entity is a Draft-Entity
                    // case 1: saved entity is a draft and save wants to publish
                    // case 2: new data is set to not publish, but we don't want a branch
                    int? resetId = default;
                    if (stateChanged || hasAdditionalDraft)
                    {
                        // now reset the branch/entity-state to properly set the state / purge the draft
                        dbEnt = DbContext.Publishing.ClearDraftBranchAndSetPublishedState(dbEnt,
                            existingDraftId,
                            newEnt.IsPublished);

                        // update ID of the save-entity, as it's used again later on...
                        resetId = dbEnt.EntityId;
                        //newEnt.ResetEntityId(dbEnt.EntityId);
                    }

                    #endregion

                    // update transactionId modified for the DB record
                    dbEnt.TransModifiedId = transactionId;

                    // increase version
                    dbEnt.Version++;
                    //newEnt = _factory.Entity.ResetIdentifiers(newEnt, version: dbEnt.Version);
                    newEnt = builder.Entity.CreateFrom(newEnt, id: resetId, version: dbEnt.Version);

                    // prepare export for save json OR versioning later on
                    jsonExport = Serializer.Serialize(newEnt);

                    if (saveJson)
                    {
                        dbEnt.Json = jsonExport;
                        dbEnt.ContentTypeId = contentTypeId; // in case the previous entity wasn't json stored yet
                        dbEnt.ContentType = newEnt.Type.NameId; // in case the previous entity wasn't json stored yet
                    }
                    // super exotic case - maybe it was a json before, but isn't any more...
                    // this probably only happens on the master system, where we maintain the 
                    // core content-types like @All
                    // In this case we must reset this, otherwise the next load will still prefer the json
                    else
                    {
                        if (dbEnt.ContentTypeId == DbConstant.RepoIdForJsonEntities)
                            dbEnt.ContentTypeId = contentTypeId;
                        if (dbEnt.Json != null)
                            dbEnt.Json = null;
                        if (dbEnt.ContentType != null)
                            dbEnt.ContentType = null;
                    }

                    // first, clean up all existing attributes / values (flush)
                    // this is necessary after remove, because otherwise EF state tracking gets messed up
                    DbContext.DoAndSave(
                        () => dbEnt.TsDynDataValues.Clear(),
                        "Flush values"
                    );
                });

                #endregion Step 3b
            }

            #endregion Step 3

            #region Step 4: Save all normal attributes / values & relationships

            if (!saveJson)
            {
                // save all the values we just added
                SaveAttributesAsEav(newEnt, so, attributeDefs, dbEnt, zoneLangs, transactionId, logDetails);
                DbContext.Relationships.ChangeRelationships(newEnt, dbEnt, attributeDefs, so);
            }
            else if (isNew)
                if (logDetails)
                    l.A("won't save properties / relationships in db model as it's json");
                else
                    DropEavAttributesForJsonItem(newEnt);

            #endregion


            #region Step 6: Ensure versioning

            if (jsonExport == null)
                throw new("trying to save version history entry, but jsonExport isn't ready");
            DbContext.Versioning.AddToHistoryQueue(dbEnt.EntityId, dbEnt.EntityGuid, jsonExport);

            #endregion



            #region Workaround for preserving the last guid (temp - improve some day...)

            entityId = dbEnt.EntityId; // remember the ID for later
            TempLastSaveGuid = dbEnt.EntityGuid;

            #endregion

        }); // end of transaction

        return l.Return(entityId, $"done id:{entityId}");
    }

    /// <summary>
    /// Temp helper to provide the last guid to the caller
    /// this is a messy workaround, must find a better way someday...
    /// </summary>
    public Guid TempLastSaveGuid;


}