namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal class Process3New3DbStoreJson() : Process0Base("DB.EPr3n3")
{
    public override EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data)
    {
        // Only update the header for JSON on new, as the header with json-value changes
        // will be updated in phase 

        var l = services.LogDetails.Fn<EntityProcessData>();

        //if (!data.IsNew)
        //    return l.Return(data, "not new, skip");
        
        //if (data is { SaveJson: true, DbEntity: not null })
        //    services.DbStorage.DoAndSaveWithoutChangeDetection(() =>
        //    {
        //        if (string.IsNullOrEmpty(data.JsonExport))
        //            throw new ArgumentNullException(nameof(data.JsonExport));

        //        data.DbEntity!.Json = data.JsonExport;
        //        data.DbEntity.ContentType = data.NewEntity.Type.NameId;

        //        services.DbStorage.SqlDb.Update(data.DbEntity);
        //    }, "update json");

        return l.Return(data);
    }

}
