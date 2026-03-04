using ToSic.Eav.Repository.Efc.Sys.DbEntities;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;

/// <summary>
/// Step 3b: Check published (only if not new) - make sure we don't have multiple drafts
/// </summary>
internal class Process3Upd2PrepareUpdate(): Process0Base("Db.EPr3u2")
{
    public override EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data)
    {
        // Track if we make changes to the header to decide later if we need to save again
        var headerNeedsUpdate = false;

        var dbEnt = data.DbEntity!;
        var newEnt = data.NewEntity;

        var l = services.LogDetails
            .Fn<EntityProcessData>($"used existing i:{dbEnt.EntityId}, " +
                                   $"guid:{dbEnt.EntityGuid}, " +
                                   $"newState:{newEnt.IsPublished}, state-changed:{data.StateChanged}, " +
                                   $"has-additional-draft:{data.HasAdditionalDraft}");

        // If Update, then check draft etc.
        if (!data.IsNew)
        {

            #region If draft but should be published, correct what's necessary

            // Update as Published but Current Entity is a Draft-Entity
            // case 1: saved entity is a draft and save wants to publish
            // case 2: new data is set to not publish, but we don't want a branch
            int? resetId = default;
            if (data.StateChanged || data.HasAdditionalDraft)
            {
                var publishedBefore = dbEnt.IsPublished;
                // now reset the branch/entity-state to properly set the state / purge the draft
                dbEnt = services.DbStorage.Publishing.ClearDraftBranchAndSetPublishedState(dbEnt, data.ExistingDraftId,
                    data.NewEntity.IsPublished);

                // update ID of the save-entity, as it's used again later on...
                resetId = dbEnt.EntityId;

                if (dbEnt.IsPublished != publishedBefore)
                    headerNeedsUpdate = true;
            }

            #endregion

            // update transactionId modified for the DB record
            dbEnt.TransModifiedId = services.TransactionId;

            // increase version
            dbEnt.Version++;
            //newEnt = _factory.Entity.ResetIdentifiers(newEnt, version: dbEnt.Version);
            newEnt = services.DataAssembler.Entity.CreateFrom(newEnt, id: resetId, version: dbEnt.Version);

            headerNeedsUpdate = true;
        }

        // prepare export for save json OR versioning later on
        data = data with
        {
            NewEntity = newEnt,
            JsonExport = services.Serializer.Serialize(newEnt)
        };

        if (data.SaveJson)
        {
            dbEnt.Json = data.JsonExport;
            dbEnt.ContentTypeId = data.ContentTypeId; // in case the previous entity wasn't json stored yet
            dbEnt.ContentType = newEnt.Type.NameId; // in case the previous entity wasn't json stored yet
            headerNeedsUpdate = true;
        }
        // super exotic case - maybe it was a json before, but isn't any more...
        // this probably only happens on the master system, where we maintain the 
        // core content-types like @All
        // In this case we must reset this, otherwise the next load will still prefer the json
        else
        {
            if (dbEnt.ContentTypeId == DbConstant.RepoIdForJsonEntities)
            {
                dbEnt.ContentTypeId = data.ContentTypeId;
                headerNeedsUpdate = true;
            }

            if (dbEnt.Json != null)
            {
                dbEnt.Json = null;
                headerNeedsUpdate = true;
            }

            if (dbEnt.ContentType != null)
            {
                dbEnt.ContentType = null;
                headerNeedsUpdate = true;
            }
        }

        data = data with
        {
            DbEntity = dbEnt,
            HeaderNeedsUpdate = headerNeedsUpdate,
        };

        return l.Return(data);
    }

    // TODO: PROBABLY MOVE INTO OWN PROCESS
    public override ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data, bool logProcess)
    {
        var l = GetLogCall(services, logProcess);
        // Skip if all are NOT new
        //if (data.All(d => d.IsNew))
        //    return l.Return(data, "all new, skip");
        
        // Process each item
        data = data
            .Select(d => ProcessOne(services, d))
            .ToListOpt();

        // If any of them must be stored as JSON, then save the headers again with the updated JSON
        // This is a bit confusing, since it already happens for new entities before
        // we should probably sync this so it's less confusing
        var updates = data
            .Where(d => d.HeaderNeedsUpdate)
            .Select(d => d.DbEntity!)
            .ToListOpt();

        if (updates.Any())
            services.DbStorage.DoAndSaveWithoutChangeDetection(() => services.DbStorage.SqlDb.UpdateRange(updates), "update json");

        return l.Return(data);
    }
}
