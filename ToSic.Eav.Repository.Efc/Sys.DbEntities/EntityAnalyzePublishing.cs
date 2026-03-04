using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;
internal class EntityAnalyzePublishing(DbStorage.DbStorage dbStorage, DataAssembler dataAssembler, ICollection<IEntityPair<SaveOptions>> entityOptionPairs, ILog? log) : HelperBase(log, "Db.AzPubl")
{
    [field: AllowNull, MaybeNull]
    private Dictionary<int, int?> EntityDraftMapCache => field ??= dbStorage.Publishing
        .GetDraftBranchMap(entityOptionPairs.Select(e => e.Entity.EntityId).ToList());


    /// <summary>
    /// Get the draft-id and branching info, 
    /// then correct branching-infos on the entity depending on the scenario
    /// </summary>
    /// <param name="newEnt">the entity to be saved, with IDs and Guids</param>
    /// <param name="so"></param>
    /// <param name="logDetails"></param>
    /// <returns></returns>
    internal (int? ExistingDraftId, bool HasDraft, IEntity Entity) GetDraftAndCorrectIdAndBranching(IEntity newEnt,
        SaveOptions so, bool logDetails)
    {
        var l = Log.Fn<(int?, bool, IEntity)>($"entity:{newEnt.EntityId}", timer: true);

        // If ID == 0, it's new, so only continue, if we were given an EntityId
        if (newEnt.EntityId <= 0)
            return l.Return((null, false, newEnt), "entity id <= 0 means new, so skip draft lookup");

        if (logDetails)
            l.A("entity id > 0 - will check draft/branching");

        // find a draft of this - note that it won't find anything, if the item itself is the draft
        if (EntityDraftMapCache == null)
            throw new("Needs cached list of draft-branches, but list is null");
        if (!EntityDraftMapCache.TryGetValue(newEnt.EntityId, out var existingDraftId))
            throw new("Expected item to be preloaded in draft-branching map, but not found");

        // only true, if there is an "attached" draft; false if the item itself is draft
        var hasDraft = existingDraftId != null && newEnt.EntityId != existingDraftId;

        if (logDetails)
            l.A($"draft check: id:{newEnt.EntityId} {nameof(existingDraftId)}:{existingDraftId}, {nameof(hasDraft)}:{hasDraft}");

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
        var entityInDb = dbStorage.Entities.GetDbEntityStub(newEnt.EntityId);
        if (!entityInDb.IsPublished)
        {
            if (logDetails)
                l.A("original in DB is not published, will overwrite and not branch again");
            return l.Return((existingDraftId, hasDraft, newEnt), existingDraftId?.ToString() ?? "null");
        }

        if (logDetails)
            l.A("original is published, so we'll draft in a branch");
        var clone = dataAssembler.Entity.CreateFrom(newEnt,
            publishedId: newEnt.EntityId, // set this, in case we'll create a new one
            id: existingDraftId ?? 0  // set to the draft OR 0 = new
        );

        return l.Return((existingDraftId,
                false, // not additional anymore, as we're now pointing this as primary
                clone),
            existingDraftId?.ToString() ?? "null");
    }

}
