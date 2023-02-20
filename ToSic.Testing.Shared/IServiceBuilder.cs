namespace ToSic.Testing.Shared
{
    public interface IServiceBuilder
    {
        T GetService<T>();
    }
}
