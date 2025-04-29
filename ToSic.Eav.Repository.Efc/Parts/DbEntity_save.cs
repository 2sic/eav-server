namespace ToSic.Eav.Repository.Efc.Parts;

partial class DbEntity
{
    public const int RepoIdForJsonEntities = 1;
    private const int MaxToLogDetails = 10;

    private List<DimensionDefinition> _zoneLangs;


    private int SaveEntity(IEntityPair<SaveOptions> entityOptionPair, bool logDetails)
    {
        var newEnt = entityOptionPair.Entity;
        var so = entityOptionPair.Partner;
        var l = Log.Fn<int>($"id:{newEnt?.EntityId}/{newEnt?.EntityGuid}, logDetails:{logDetails}");

        #region Step 1: Do some initial error checking and preparations

        if (newEnt == null) throw new ArgumentNullException(nameof(newEnt));

        if (newEnt.Type == null)
            throw new("trying to save entity without known content-type, cannot continue");

        #region Test what languages are given, and check if they exist in the target system

        // continue here - must ensure that the languages are passed in, cached - or are cached on the DbEntity... for multiple saves
        if (_zoneLangs == null)
            _zoneLangs = so.Languages ?? throw new("languages missing in save-options. cannot continue");

        var usedLanguages = newEnt.GetUsedLanguages();
        if (usedLanguages.Count > 0)
            if (!usedLanguages.All(lang => _zoneLangs.Any(zl => zl.Matches(lang.Key))))
                throw new(
                    $"entity has languages which are not in zone - entity has {usedLanguages.Count} zone has {_zoneLangs.Count} " +
                    $"used-list: '{string.Join(",", usedLanguages.Select(lang => lang.Key).ToArray())}'");

        if (logDetails)
        {
            l.A($"lang checks - zone language⋮{_zoneLangs.Count}, usedLanguages⋮{usedLanguages.Count}");
            l.A(l.Try(() =>
                $"langs zone:[{string.Join(",", _zoneLangs.Select(z => z.EnvironmentKey))}] used:[{string.Join(",", usedLanguages.Select(u => u.Key))}]"));
        }


        #endregion Test languages exist

        // check if saving should be with db-type or with the plain json
        var saveJson = UseJson(newEnt);
        string jsonExport = null;
        if (logDetails) l.A($"save json:{saveJson}");

        #endregion Step 1



        #region Step 2: check header record - does it already exist, what ID should we use, etc.

        // If we think we'll update an existing entity...
        // ...we have to check if we'll actually update the draft of the entity
        // ...or create a new draft (branch)
        var (existingDraftId, hasAdditionalDraft, entity) = GetDraftAndCorrectIdAndBranching(newEnt, so, logDetails);
        newEnt = entity; // may have been replaced with an updated IEntity during corrections

        var isNew = newEnt.EntityId <= 0; // remember how we want to work...
        if (logDetails) l.A($"entity id:{newEnt.EntityId} - will treat as new:{isNew}");

        var (contentTypeId, attributeDefs) = GetContentTypeAndAttribIds(saveJson, newEnt, logDetails);

        #endregion Step 2



        ToSicEavEntities dbEnt = null;

        var changeLogId = DbContext.Versioning.GetChangeLogId();

        DbContext.DoInTransaction(() =>
        {
            #region Step 3: either create a new entity, or if it's an update, do draft/published checks to ensure correct data

            // is New
            if (isNew)
                l.Do("Create new...", l2 =>
                {
                    if (newEnt.EntityGuid == Guid.Empty)
                    {
                        if (logDetails) l2.A("New entity guid was null, will throw exception");
                        throw new ArgumentException(
                            "can't create entity in DB with guid null - entities must be fully prepared before sending to save");
                    }

                    dbEnt = CreateDbRecord(newEnt, changeLogId, contentTypeId);
                    // update the ID - for versioning and/or json persistence
                    newEnt = builder.Entity.CreateFrom(newEnt, id: dbEnt.EntityId);
                    //newEnt.ResetEntityId(dbEnt.EntityId); // update this, as it was only just generated

                    // prepare export for save json OR versioning later on
                    jsonExport = GenerateJsonOrReportWhyNot(newEnt, logDetails);

                    if (saveJson)
                        l2.Do($"id:{newEnt.EntityId}, guid:{newEnt.EntityGuid}", () =>
                        {
                            dbEnt.Json = jsonExport;
                            dbEnt.ContentType = newEnt.Type.NameId;
                            DbContext.DoAndSaveWithoutChangeDetection(() => DbContext.SqlDb.Update(dbEnt),
                                "update json");
                        });
                    return $"i:{dbEnt.EntityId}, guid:{dbEnt.EntityGuid}";
                });
            // is Update
            else
            {
                #region Step 3b: Check published (only if not new) - make sure we don't have multiple drafts

                // new: always change the draft if there is one! - it will then either get published, or not...
                dbEnt = DbContext.Entities
                    .GetDbEntityFull(newEnt.EntityId); // get the published one (entityId is always the published id)

                var stateChanged = dbEnt.IsPublished != newEnt.IsPublished;
                var paramsMsg =
                    $"used existing i:{dbEnt.EntityId}, guid:{dbEnt.EntityGuid}, newstate:{newEnt.IsPublished}, state-changed:{stateChanged}, has-additional-draft:{hasAdditionalDraft}";
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

                    // update changelog modified for the DB record
                    dbEnt.ChangeLogModified = changeLogId;

                    // increase version
                    dbEnt.Version++;
                    //newEnt = _factory.Entity.ResetIdentifiers(newEnt, version: dbEnt.Version);
                    newEnt = builder.Entity.CreateFrom(newEnt, id: resetId, version: dbEnt.Version);

                    // prepare export for save json OR versioning later on
                    jsonExport = Serializer.Serialize(newEnt);

                    if (saveJson)
                    {
                        dbEnt.Json = jsonExport;
                        dbEnt.AttributeSetId = contentTypeId; // in case the previous entity wasn't json stored yet
                        dbEnt.ContentType = newEnt.Type.NameId; // in case the previous entity wasn't json stored yet
                    }
                    // super exotic case - maybe it was a json before, but isn't any more...
                    // this probably only happens on the master system, where we maintain the 
                    // core content-types like @All
                    // In this case we must reset this, otherwise the next load will still prefer the json
                    else
                    {
                        if (dbEnt.AttributeSetId == RepoIdForJsonEntities) dbEnt.AttributeSetId = contentTypeId;
                        if (dbEnt.Json != null) dbEnt.Json = null;
                        if (dbEnt.ContentType != null) dbEnt.ContentType = null;
                    }

                    // first, clean up all existing attributes / values (flush)
                    // this is necessary after remove, because otherwise EF state tracking gets messed up
                    DbContext.DoAndSave(() => dbEnt.ToSicEavValues.Clear(), "Flush values");
                });

                #endregion Step 3b
            }

            #endregion Step 3

            #region Step 4: Save all normal attributes / values & relationships

            if (!saveJson)
            {
                // save all the values we just added
                SaveAttributesAsEav(newEnt, so, attributeDefs, dbEnt, changeLogId, logDetails);
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
                throw new("trying to save version history entry, but jsonexport isn't ready");
            DbContext.Versioning.AddToHistoryQueue(dbEnt.EntityId, dbEnt.EntityGuid, jsonExport);

            #endregion



            #region Workaround for preserving the last guid (temp - improve some day...)

            TempLastSaveGuid = dbEnt.EntityGuid;

            #endregion

        }); // end of transaction

        return l.Return(dbEnt.EntityId, "done id:" + dbEnt?.EntityId);
    }


    /// <summary>
    /// Get the draft-id and branching info, 
    /// then correct branching-infos on the entity depending on the scenario
    /// </summary>
    /// <param name="newEnt">the entity to be saved, with IDs and Guids</param>
    /// <param name="so"></param>
    /// <param name="logDetails"></param>
    /// <returns></returns>
    private (int? ExistingDraftId, bool HasDraft, IEntity Entity) GetDraftAndCorrectIdAndBranching(IEntity newEnt,
        SaveOptions so, bool logDetails) 
    {
        var l = Log.Fn<(int?, bool, IEntity)>($"entity:{newEnt.EntityId}", timer: true);

        // If ID == 0, it's new, so only continue, if we were given an EntityId
        if (newEnt.EntityId <= 0)
            return l.Return((null, false, newEnt), "entity id <= 0 means new, so skip draft lookup");

        if (logDetails)
            l.A("entity id > 0 - will check draft/branching");

        // find a draft of this - note that it won't find anything, if the item itself is the draft
        if (_entityDraftMapCache == null)
            throw new("Needs cached list of draft-branches, but list is null");
        if (!_entityDraftMapCache.TryGetValue(newEnt.EntityId, out var existingDraftId))
            throw new("Expected item to be preloaded in draft-branching map, but not found");

        // only true, if there is an "attached" draft; false if the item itself is draft
        var hasDraft = existingDraftId != null && newEnt.EntityId != existingDraftId; 

        if (logDetails)
            l.A($"draft check: id:{newEnt.EntityId} {nameof(existingDraftId)}:{existingDraftId}, {nameof(hasDraft)}:{hasDraft}");

        // #WipDraftShouldBranch
        //var placeDraftInBranch = ((Entity)newEnt).PlaceDraftInBranch;
        var placeDraftInBranch = so.DraftShouldBranch;

        // if it's being saved as published, or the draft will be without an old original, then exit 
        if (newEnt.IsPublished || !placeDraftInBranch)
        {
            if (logDetails)
                l.A($"new is published or branching is not wanted, so we won't branch - returning draft-id:{existingDraftId}");
            return l.Return((existingDraftId, hasDraft, newEnt), existingDraftId?.ToString() ?? "null");
        }

        if (logDetails)
            l.A($"will save as draft, and setting is PlaceDraftInBranch:true");

        if (logDetails)
            l.A($"Will look for original {newEnt.EntityId} to check if it's not published.");
        // check if the original is also not published, with must prevent a second branch!
        var entityInDb = DbContext.Entities.GetDbEntityStub(newEnt.EntityId);
        if (!entityInDb.IsPublished)
        {
            if (logDetails)
                l.A("original in DB is not published, will overwrite and not branch again");
            return l.Return((existingDraftId, hasDraft, newEnt), existingDraftId?.ToString() ?? "null");
        }

        if (logDetails)
            l.A("original is published, so we'll draft in a branch");
        var clone = builder.Entity.CreateFrom(newEnt,
            publishedId: newEnt.EntityId, // set this, in case we'll create a new one
            id: existingDraftId ?? 0  // set to the draft OR 0 = new
        ) as Entity;

        return l.Return((existingDraftId,
                false, // not additional anymore, as we're now pointing this as primary
                clone),
            existingDraftId?.ToString() ?? "null");
    }

    private Dictionary<int, int?> _entityDraftMapCache;


    /// <summary>
    /// Temp helper to provide the last guid to the caller
    /// this is a messy workaround, must find a better way someday...
    /// </summary>
    public Guid TempLastSaveGuid;


}