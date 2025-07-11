namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal class Process0Base(string logName): HelperBase(null, logName), IEntityProcess
{
    public virtual EntityProcessData Process(EntityProcessServices services, EntityProcessData data)
    {
        this.LinkLog(services.LogDetails);
        return data;
    }
}
