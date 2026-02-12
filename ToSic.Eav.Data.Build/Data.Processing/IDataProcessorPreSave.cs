namespace ToSic.Eav.Data.Processing;

public interface IDataProcessorPreSave
{
    Task<DataProcessorResult<IEntity?>> Process(DataProcessorResult<IEntity?> entity);
}
