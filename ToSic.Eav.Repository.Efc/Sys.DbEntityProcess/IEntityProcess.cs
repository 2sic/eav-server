namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal interface IEntityProcess //: IHasLog
{
    ICollection<EntityProcessData> Process(EntityProcessServices services, ICollection<EntityProcessData> data, bool logProcess);
}
