namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal class Process3New3DbStoreJson() : Process0Base("DB.EPr3n3")
{
    public override EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data)
    {
        var l = services.LogDetails.Fn<EntityProcessData>();

        if (!data.IsNew)
            return l.Return(data, "not new, skip");
        
        if (data is { SaveJson: true, DbEntity: not null })
            services.DbStorage.DoAndSaveWithoutChangeDetection(() => services.DbStorage.SqlDb.Update(data.DbEntity), "update json");

        return l.Return(data);
    }

}
