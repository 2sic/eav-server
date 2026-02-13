namespace ToSic.Eav.Data.Processing;

/// <summary>
/// WIP - idea is to have objects which can process data - like before/after saving.
/// Specs still very unclear; 2dm.
/// </summary>
[PrivateApi("WIP v21")]
public interface IDataProcessor
{
    Task<DataProcessorResult<IEntity?>> Process(string action, DataProcessorResult<IEntity?> entity);
}
