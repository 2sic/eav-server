namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;

/// <summary>
/// Step 3b: Check published (only if not new) - make sure we don't have multiple drafts
/// </summary>
internal class Process3Upd3ClearValues(): Process0Base("Db.EPr3u3")
{
    public override EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data)
        => throw new NotSupportedException("Single item call not supported");

    //{
    //    if (data.IsNew)
    //        return data;
    //    var l = services.LogDetails.Fn<EntityProcessData>();
    //    // first, clean up all existing attributes / values (flush)
    //    // this is necessary after remove, because otherwise EF state tracking gets messed up
    //    services.DbStorage.DoAndSave(
    //        () => data.DbEntity!.TsDynDataValues.Clear(),
    //        "Flush values"
    //    );
    //    return l.Return(data);
    //}

    public override ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data, bool logProcess)
    {
        var l = GetLogCall(services, logProcess);

        // Skip if all are new
        if (data.All(d => d.IsNew))
            return l.Return(data, "all new, none update, skip");

        // first, clean up all existing attributes / values (flush)
        // this is necessary after remove, because otherwise EF state tracking gets messed up
        services.DbStorage.DoAndSave(
            () =>
            {
                foreach (var entityProcessData in data.Where(d => !d.IsNew))
                    entityProcessData.DbEntity!.TsDynDataValues.Clear();
            },
            "Flush values"
        );

        return l.Return(data);

    }
}
