namespace ToSic.Lib.DI;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class LazyLike<TService>(TService value) : ILazyLike<TService>
{
    public TService Value { get; } = value;
    public bool IsValueCreated => true;
}