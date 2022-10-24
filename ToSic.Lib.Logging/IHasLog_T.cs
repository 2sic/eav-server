namespace ToSic.Lib.Logging
{
    public interface IHasLog<out T>: IHasLog
    {
        T Init(ILog parent);
    }
}
