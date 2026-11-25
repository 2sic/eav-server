namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal class Process3New3DbStoreJson: IEntityProcess
{
    public EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data)
    {
        // Only update the header for JSON on new, as the header with json-value changes
        // will be updated in phase 

        var l = services.LogDetails.Fn<EntityProcessData>();

        return l.Return(data);
    }

    public ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data, bool logProcess)
        => this.Process(services, data, logProcess, ProcessOne);

}
