namespace ToSic.Eav.Data.Shared
{
    public interface IShareSource<out T>: IWrapper<T>, IAncestor
    {
    }
}
