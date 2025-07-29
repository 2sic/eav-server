namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal class Process3New2DbStoreHeader() : Process0Base("DB.EPr3n2")
{
    public override EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data)
        => throw new NotSupportedException("Single item call not supported");

    //{
    //    var l = services.LogDetails.Fn<EntityProcessData>();
    //    if (!data.IsNew)
    //        return l.Return(data, "not new, skip");
    //    data = data with
    //    {
    //        DbEntity = services.DbEntity.CreateDbRecord(data.NewEntity, services.TransactionId, data.ContentTypeId)
    //    };
    //    services.DbEntity.SaveNew([data.DbEntity]);
    //    // update the ID - for versioning and/or json persistence
    //    data = data with { NewEntity = services.Builder.Entity.CreateFrom(data.NewEntity, id: data.DbEntity.EntityId) };
    //    // prepare export for save json OR versioning later on
    //    data = data with { JsonExport = services.DbEntity.GenerateJsonOrReportWhyNot(data.NewEntity, data.LogDetails) };
    //    if (data.SaveJson)
    //    {
    //        data.DbEntity.Json = data.JsonExport;
    //        data.DbEntity.ContentType = data.NewEntity.Type.NameId;
    //    }
    //    return l.Return(data, $"i:{data.DbEntity.EntityId}, guid:{data.DbEntity.EntityGuid}");
    //}

    public override ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data, bool logProcess)
    {
        var l = GetLogCall(services, logProcess);

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

        // Save the new ones
        var newEntities = data
            .Where(d => d.IsNew)
            .Select(d => d.DbEntity!);
        services.DbEntity.SaveCreatedNoChangeDetection(newEntities);

        // Update the IDs in the NewEntity for versioning and/or json persistence
        data = data
            .Select(d =>
            {
                if (!d.IsNew)
                    return d; // skip, not new

                // Update the IDs in the NewEntity for versioning and/or json persistence
                var updated = services.Builder.Entity.CreateFrom(d.NewEntity, id: d.DbEntity!.EntityId);
                // Prepare export for save json OR versioning later on
                var jsonExport = services.DbEntity.GenerateJsonOrReportWhyNot(updated, d.LogDetails);

                // If we plan to save the JSON, update the DbEntity
                if (d.SaveJson)
                {
                    d.DbEntity!.Json = jsonExport;
                    d.DbEntity.ContentType = updated.Type.NameId;
                }

                return d with
                {
                    NewEntity = updated,
                    JsonExport = jsonExport
                };
            })
            .ToListOpt();


        return l.Return(data);
    }
}
