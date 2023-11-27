namespace ToSic.Lib.DI;

public class LazyLike<TService>: ILazyLike<TService>
{
    public LazyLike(TService value) => Value = value;
    public TService Value { get; }
    public bool IsValueCreated => true;
}