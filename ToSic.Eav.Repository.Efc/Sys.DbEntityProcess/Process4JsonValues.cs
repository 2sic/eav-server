namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;


internal class Process4JsonValues(): Process0Base("Db.EPr4JV")
{
    public override EntityProcessData Process(EntityProcessServices services, EntityProcessData data)
    {
        if (!data.SaveJson)
            return data;

        base.Process(services, data);
        var l = services.LogDetails.Fn<EntityProcessData>();

        // careful - this is different from before, but I believe the previous one was wrong...
        // but we didn't notice, because we didn't log details...
        if (data.IsNew)
        {
            if (data.LogDetails)
                l.A("won't save properties / relationships in db model as it's json");
        }
        else
            services.DbEntity.DropEavAttributesForJsonItem(data.NewEntity);

        return l.Return(data);
    }
}
