using Microsoft.EntityFrameworkCore;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;


internal class Process5TableRelationships(): Process0Base("Db.EPr5TR")
{
    public override EntityProcessData Process(EntityProcessServices services, EntityProcessData data)
    {
        if (data.SaveJson)
            return data;

        base.Process(services, data);
        var l = services.LogDetails.Fn<EntityProcessData>();

        // save all the values we just added
        services.DbStorage.Relationships.ChangeRelationships(data.NewEntity, data.DbEntity!.EntityId, data.AttributeDefs, data.Options);

        return l.Return(data);
    }
}
