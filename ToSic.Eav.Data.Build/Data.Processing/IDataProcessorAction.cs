namespace ToSic.Eav.Data.Processing;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IDataProcessorAction
{
    Task<DataProcessorResult<IEntity?>> Process(DataProcessorResult<IEntity?> entity);

}
