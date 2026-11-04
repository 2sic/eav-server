namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;


internal class Process5TableRelationships: IEntityProcess
{
    public ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data, bool logProcess)
    {
        var l = this.GetLogCall(services, logProcess);

        if (data.All(d => d.SaveJson))
            return l.Return(data, "all SaveJson, none update, skip");

        var relChanges = data
            .Where(d => !d.SaveJson)
            .SelectMany(d => services.DbStorage.Relationships.GetChangeRelationships(d.NewEntity, d.DbEntity!.EntityId, d.AttributeDefs!, d.Options))
            .ToListOpt();

        services.DbStorage.Relationships.ImportRelationshipQueueAndSaveUntracked(relChanges);

        return l.Return(data);
    }
}
