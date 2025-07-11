namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;

/// <summary>
/// Step 3b: Check published (only if not new) - make sure we don't have multiple drafts
/// </summary>
internal class Process3Upd3ClearValues(): Process0Base("Db.EPr3u3")
{
    public override EntityProcessData Process(EntityProcessServices services, EntityProcessData data)
    {
        if (data.IsNew)
            return data;

        base.Process(services, data);
        var l = services.LogDetails.Fn<EntityProcessData>();

        // first, clean up all existing attributes / values (flush)
        // this is necessary after remove, because otherwise EF state tracking gets messed up
        services.DbStorage.DoAndSave(
            () => data.DbEntity!.TsDynDataValues.Clear(),
            "Flush values"
        );

        return l.Return(data);
    }
}
