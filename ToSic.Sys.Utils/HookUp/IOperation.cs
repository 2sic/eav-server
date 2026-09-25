namespace ToSic.Sys.HookUp;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IOperation;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IOperation<in TIn, out TOut> : IOperation
{
    TOut Process(TIn input);
}

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IOperation<T>: IOperation<T, T>;