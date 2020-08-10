namespace ToSic.Eav.Logging
{
    public interface IHasLog<out T>
    {
        T Init(ILog parent);
    }
}
