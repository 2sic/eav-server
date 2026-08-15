namespace ToSic.Eav.Data.Processing;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IDataProcessorAction
{
    Task<ActionData<IEntity?>> Process(ActionData<IEntity?> entity);

}
