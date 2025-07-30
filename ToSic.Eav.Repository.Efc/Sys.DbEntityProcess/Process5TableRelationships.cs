namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;


internal class Process5TableRelationships(): Process0Base("Db.EPr5TR")
{
    public override EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data)
        => throw new NotSupportedException("Single item call not supported");

    //{
    //    if (data.SaveJson)
    //        return data;

    //    var l = services.LogDetails.Fn<EntityProcessData>();

    //    // save all the values we just added
    //    services.DbStorage.Relationships.ChangeRelationships(data.NewEntity, data.DbEntity!.EntityId, data.AttributeDefs, data.Options);

    //    return l.Return(data);
    //}

    public override ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data, bool logProcess)
    {
        var l = GetLogCall(services, logProcess);

        if (data.All(d => d.SaveJson))
            return l.Return(data, "all SavoJson, none update, skip");

        var relChanges = data
            .Where(d => !d.SaveJson)
            .SelectMany(d => services.DbStorage.Relationships.GetChangeRelationships(d.NewEntity, d.DbEntity!.EntityId, d.AttributeDefs, d.Options))
            .ToListOpt();

        services.DbStorage.Relationships.ImportRelationshipQueueAndSaveUntracked(relChanges);

        return l.Return(data);
    }
}
