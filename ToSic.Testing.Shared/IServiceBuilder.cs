namespace ToSic.Testing.Shared
{
    public interface IServiceBuilder
    {
        T Build<T>();
    }
}
