namespace ToSic.Sys.HookUp;

/// <summary>
/// WIP a unit of work
/// </summary>
/// <typeparam name="TDataIn"></typeparam>
/// <typeparam name="TDataOut"></typeparam>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IWork<TDataIn, TDataOut>
{
    public Task<Package<TDataOut>> Handle(WorkContext mainCtx, Package<TDataIn> package);
}

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IWork<TData>: IWork<TData, TData>;
