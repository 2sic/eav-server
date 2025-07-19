using ToSic.Eav.Repository.Efc.Sys.DbEntities;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;

/// <summary>
/// Step 3b: Check published (only if not new) - make sure we don't have multiple drafts
/// </summary>
internal class Process3Upd2PrepareUpdate(): Process0Base("Db.EPr3u2")
{
    public override EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data)
    {
        if (data.IsNew)
            return data;


        var dbEnt = data.DbEntity!;
        var newEnt = data.NewEntity;

        var l = services.LogDetails
            .Fn<EntityProcessData>($"used existing i:{dbEnt.EntityId}, " +
                                   $"guid:{dbEnt.EntityGuid}, " +
                                   $"newState:{newEnt.IsPublished}, state-changed:{data.StateChanged}, " +
                                   $"has-additional-draft:{data.HasAdditionalDraft}");

        #region If draft but should be published, correct what's necessary

        // Update as Published but Current Entity is a Draft-Entity
        // case 1: saved entity is a draft and save wants to publish
        // case 2: new data is set to not publish, but we don't want a branch
        int? resetId = default;
        if (data.StateChanged || data.HasAdditionalDraft)
        {
            // now reset the branch/entity-state to properly set the state / purge the draft
            dbEnt = services.DbStorage.Publishing.ClearDraftBranchAndSetPublishedState(dbEnt, data.ExistingDraftId, data.NewEntity.IsPublished);

            // update ID of the save-entity, as it's used again later on...
            resetId = dbEnt.EntityId;
        }

        #endregion

        // update transactionId modified for the DB record
        dbEnt.TransModifiedId = services.TransactionId;

        // increase version
        dbEnt.Version++;
        //newEnt = _factory.Entity.ResetIdentifiers(newEnt, version: dbEnt.Version);
        newEnt = services.Builder.Entity.CreateFrom(newEnt, id: resetId, version: dbEnt.Version);

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
        }
        // super exotic case - maybe it was a json before, but isn't any more...
        // this probably only happens on the master system, where we maintain the 
        // core content-types like @All
        // In this case we must reset this, otherwise the next load will still prefer the json
        else
        {
            if (dbEnt.ContentTypeId == DbConstant.RepoIdForJsonEntities)
                dbEnt.ContentTypeId = data.ContentTypeId;
            if (dbEnt.Json != null)
                dbEnt.Json = null;
            if (dbEnt.ContentType != null)
                dbEnt.ContentType = null;
        }

        data = data with { DbEntity = dbEnt };

        return l.Return(data);
    }
}
