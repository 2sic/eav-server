namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal class Process3New1LastChecks() : Process0Base("DB.EPr3n1")
{
    public override EntityProcessData Process(EntityProcessServices services, EntityProcessData data)
    {
        base.Process(services, data);
        var l = services.LogDetails.Fn<EntityProcessData>();

        if (!data.IsNew)
            return l.Return(data, "not new, not my job");

        if (data.NewEntity.EntityGuid == Guid.Empty)
        {
            if (data.LogDetails)
                l.A("New entity guid was null, will throw exception");
            return l.Return(data with
            {
                Exception = new ArgumentException("can't create entity in DB with guid null - entities must be fully prepared before sending to save"),
            });
        }

        return l.Return(data);
    }

}
