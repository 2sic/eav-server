namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal interface IEntityProcess: IHasLog
{
    //public EntityProcessData Process(EntityProcessServices services, EntityProcessData data);
    ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data);
}
