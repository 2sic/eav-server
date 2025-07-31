namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;

internal class Process4TableValues(): Process0Base("Db.EPr4TV")
{
    public override EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data)
        => throw new NotSupportedException("Single item call not supported");

    //{
    //    if (data.SaveJson)
    //        return data;

    //    var l = services.LogDetails.Fn<EntityProcessData>();

    //    // save all the values we just added
    //    services.DbEntity.SaveAttributesAsEavUntracked(data.NewEntity, data.Options, data.AttributeDefs, data.DbEntity!.EntityId, data.Languages, data.LogDetails);

    //    return l.Return(data);
    //}

    public override ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data, bool logProcess)
    {
        var l = GetLogCall(services, logProcess);

        if (data.All(d => d.SaveJson))
            return l.Return(data, "all SaveJson, none update, skip");

        var changes = data
            .Where(d => !d.SaveJson)
            .SelectMany(d => services.DbEntity.GetSaveAttributesAsEavUntracked(d.NewEntity, d.Options, d.AttributeDefs!, d.DbEntity!.EntityId, d.Languages, d.LogDetails))
            .ToListOpt();

        services.DbEntity.SaveAttributeChanges(changes);


        return l.Return(data);
    }
}
