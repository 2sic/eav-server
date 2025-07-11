namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal interface IEntityProcess
{
    public EntityProcessData Process(EntityProcessServices services, EntityProcessData data);
}
