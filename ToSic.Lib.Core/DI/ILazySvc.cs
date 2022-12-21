namespace ToSic.Lib.DI
{
    public interface ILazySvc<out T>: ILazyLike<T>, ILazyInitLog
    {
    }
}
