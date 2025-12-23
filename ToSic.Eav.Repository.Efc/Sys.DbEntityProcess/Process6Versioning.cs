using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Repository.Efc.Sys.DbParts;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;

/// <summary>
/// Step 3b: Check published (only if not new) - make sure we don't have multiple drafts
/// </summary>
internal class Process6Versioning(): Process0Base("Db.EPr6Vr")
{
    public override EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data)
        => throw new NotSupportedException("Single item call not supported");
    //{
    //    var l = services.LogDetails.Fn<EntityProcessData>();
    //    if (data.JsonExport == null)
    //        return data with
    //        {
    //            Exception = new("trying to save version history entry, but jsonExport isn't ready")
    //        };
    //    services.DbStorage.Versioning.AddToHistoryQueue(data.DbEntity!.EntityId, data.DbEntity!.EntityGuid, data.JsonExport);
    //    return l.Return(data);
    //}

    public override ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data, bool logProcess)
    {
        var l = GetLogCall(services, logProcess);

        // History snapshots should additionally capture inbound parent relationships (Parents).
        var itemsForHistory = data
            .Where(d => d.DbEntity != null)
            .Select(d => (
                Entity: d.NewEntity,
                EntityId: d.DbEntity!.EntityId,
                EntityGuid: d.DbEntity!.EntityGuid,
                ParentRef: DbVersioning.ParentRefForApp(d.DbEntity!.AppId)
            ))
            .ToList();

        var historyEntries = services.DbStorage.Versioning.PrepareHistoryEntriesWithInboundParents(itemsForHistory);
        services.DbStorage.Versioning.Save(historyEntries);

        var result = data.Select(d => d.JsonExport == null
                ? d with { Exception = new("trying to save version history entry, but jsonExport isn't ready") }
                : d
            )
            .ToListOpt();
        
        return l.Return(result);
    }
}
