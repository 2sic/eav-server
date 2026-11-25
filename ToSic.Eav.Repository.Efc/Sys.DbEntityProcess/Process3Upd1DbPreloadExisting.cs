namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;

/// <summary>
/// Step 3b: Check published (only if not new) - make sure we don't have multiple drafts
/// </summary>
internal class Process3Upd1DbPreloadExisting: IEntityProcess
{
    public ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data, bool logProcess)
    {
        var l = this.GetLogCall(services, logProcess);

        // Skip if all are new
        if (data.All(d => d.IsNew))
            return l.Return(data, "all new, none update, skip");


        var ids = data
            .Where(d => !d.IsNew)
            .Select(d => d.NewEntity.EntityId)
            .ToArray();

        // get the published one (entityId is always the published id)
        var dataEntities = services.DbEntity.GetDbEntitiesFullUntracked(ids);

        // new: always change the draft if there is one! - it will then either get published, or not...
        data = data
            .Select(d =>
            {
                // New was already created / added before and will be missing here
                if (d.IsNew)
                    return d;

                // for every update there MUST be an original from the DB
                var original = dataEntities.Single(ent => ent.EntityId == d.NewEntity.EntityId);
                return d with
                {
                    DbEntity = original,
                    StateChanged = original.IsPublished != d.NewEntity.IsPublished
                };
            })
            .ToListOpt();

        return l.Return(data);
    }
}
