namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal class Process3New2DbStoreNewHeaders: IEntityProcess
{
    public EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data)
        => throw new NotSupportedException("Single item call not supported");

    public ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data, bool logProcess)
    {
        var l = this.GetLogCall(services, logProcess);

        // Skip if all are NOT new
        if (data.All(d => !d.IsNew))
            return l.Return(data, "none new, skip");


        // Generate DbEntities for all new entities
        data = data
            .Select(d => !d.IsNew
                ? d // skip, not new
                : d with
                {
                    DbEntity = services.DbEntity.CreateDbRecord(d.NewEntity, services.TransactionId, d.ContentTypeId)
                })
            .ToListOpt();

        // Save the new ones, afterward the entities have IDs
        var newEntities = data
            .Where(d => d.IsNew)
            .Select(d => d.DbEntity!);

        services.DbStorage.DoAndSaveWithoutChangeDetection(() =>
        {
            services.DbStorage.SqlDb.AddRange(newEntities);
        }, "save new");

        // Update the IDs in the NewEntity for versioning and/or json persistence
        data = data
            .Select(d =>
            {
                // Update the IDs in the NewEntity for versioning and/or json persistence
                var entityWithCorrectId = d.IsNew
                    ? services.DataAssembler.Entity.CreateFrom(d.NewEntity, id: d.DbEntity!.EntityId)
                    : d.NewEntity;

                // Prepare export for save json OR versioning later on
                //var jsonExport = services.DbEntity.GenerateJsonOrReportWhyNot(entityWithCorrectId, d.LogDetails);

                return d with
                {
                    NewEntity = entityWithCorrectId,
                    //JsonExport = jsonExport
                };
            })
            .ToListOpt();


        return l.Return(data);
    }
}
