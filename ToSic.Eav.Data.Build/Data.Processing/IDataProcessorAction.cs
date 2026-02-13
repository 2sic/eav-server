namespace ToSic.Eav.Data.Processing;

public interface IDataProcessorAction
{
    Task<DataProcessorResult<IEntity?>> Process(DataProcessorResult<IEntity?> entity);

}
