namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;

internal class Process4TableValues: IEntityProcess
{
    public ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data, bool logProcess)
    {
        var l = this.GetLogCall(services, logProcess);

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
