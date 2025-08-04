namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;


internal class Process4JsonValues(): Process0Base("Db.EPr4JV")
{
    public override EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data)
    {
        if (!data.SaveJson)
            return data;

        var l = services.LogDetails.Fn<EntityProcessData>();

        // careful - this is different from before, but I believe the previous one was wrong...
        // but we didn't notice, because we didn't log details...
        if (data.IsNew)
        {
            if (data.LogDetails)
                l.A("won't save properties / relationships in db model as it's json");
        }
        else
            services.DbEntity.DropAttributesAndRelationshipsForJsonItem(data.NewEntity.EntityId);

        return l.Return(data);
    }
}
