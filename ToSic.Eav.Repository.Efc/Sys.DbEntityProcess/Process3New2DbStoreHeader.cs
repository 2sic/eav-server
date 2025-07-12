namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal class Process3New2DbStoreHeader() : Process0Base("DB.EPr3n2")
{
    public override EntityProcessData Process(EntityProcessServices services, EntityProcessData data)
    {
        base.Process(services, data);
        var l = services.LogDetails.Fn<EntityProcessData>();

        if (!data.IsNew)
            return l.Return(data, "not new, skip");

        data = data with
        {
            DbEntity = services.DbEntity.CreateDbRecord(data.NewEntity, services.TransactionId, data.ContentTypeId)
        };
        services.DbEntity.SaveNew([data.DbEntity]);

        // update the ID - for versioning and/or json persistence
        data = data with { NewEntity = services.Builder.Entity.CreateFrom(data.NewEntity, id: data.DbEntity.EntityId) };

        // prepare export for save json OR versioning later on
        data = data with { JsonExport = services.DbEntity.GenerateJsonOrReportWhyNot(data.NewEntity, data.LogDetails) };

        if (data.SaveJson)
        {
            data.DbEntity.Json = data.JsonExport;
            data.DbEntity.ContentType = data.NewEntity.Type.NameId;
        }

        return l.Return(data, $"i:{data.DbEntity.EntityId}, guid:{data.DbEntity.EntityGuid}");
    }

}
