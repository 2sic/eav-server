namespace ToSic.Eav.Plumbing.DI
{
    public interface ILazyLike<out T>
    {
        T Value { get; }

        bool IsValueCreated { get; }
    }
}
