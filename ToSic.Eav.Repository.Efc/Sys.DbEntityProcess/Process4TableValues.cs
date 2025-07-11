namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;


internal class Process4TableValues(): Process0Base("Db.EPr4TV")
{
    public override EntityProcessData Process(EntityProcessServices services, EntityProcessData data)
    {
        if (data.SaveJson)
            return data;

        base.Process(services, data);
        var l = services.LogDetails.Fn<EntityProcessData>();

        // save all the values we just added
        services.DbEntity.SaveAttributesAsEav(data.NewEntity, data.Options, data.AttributeDefs, data.DbEntity!.EntityId, data.Languages, data.LogDetails);

        return l.Return(data);
    }
}
