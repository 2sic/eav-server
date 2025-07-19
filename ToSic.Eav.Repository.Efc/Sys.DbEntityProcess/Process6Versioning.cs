namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;

/// <summary>
/// Step 3b: Check published (only if not new) - make sure we don't have multiple drafts
/// </summary>
internal class Process6Versioning(): Process0Base("Db.EPr6Vr")
{
    public override EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data)
    {
        var l = services.LogDetails.Fn<EntityProcessData>();

        if (data.JsonExport == null)
            return data with
            {
                Exception = new("trying to save version history entry, but jsonExport isn't ready")
            };

        services.DbStorage.Versioning.AddToHistoryQueue(data.DbEntity!.EntityId, data.DbEntity!.EntityGuid, data.JsonExport);


        return l.Return(data);
    }
}
