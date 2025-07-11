namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;

/// <summary>
/// Step 3b: Check published (only if not new) - make sure we don't have multiple drafts
/// </summary>
internal class Process9Template(): Process0Base("Db.EPr999999")
{
    public override EntityProcessData Process(EntityProcessServices services, EntityProcessData data)
    {
        //if (data.IsNew)
        //    return data;

        base.Process(services, data);
        var l = services.LogDetails.Fn<EntityProcessData>();
        

        return l.Return(data);
    }
}
