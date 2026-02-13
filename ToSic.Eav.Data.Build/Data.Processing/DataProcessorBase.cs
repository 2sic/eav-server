namespace ToSic.Eav.Data.Processing;

/// <summary>
/// WIP - idea is to have objects which can process data - like before/after saving.
/// Specs still very unclear; 2dm.
/// </summary>
public abstract class DataProcessorBase: IDataProcessor
{
    /// <summary>
    /// do nothing
    /// </summary>
    /// <param name="action"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public virtual Task<DataProcessorResult<IEntity?>> Process(string action, DataProcessorResult<IEntity?> entity)
    {
        return Task.FromResult(entity);
    }
}
