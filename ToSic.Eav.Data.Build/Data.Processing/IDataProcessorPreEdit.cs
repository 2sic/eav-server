namespace ToSic.Eav.Data.Processing;

public interface IDataProcessorPreEdit
{
    Task<DataProcessorResult<IEntity?>> Process(DataProcessorResult<IEntity?> entity);
}
