namespace ToSic.Lib.DI;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LazyLike<TService>(TService value) : ILazyLike<TService>
{
    public TService Value { get; } = value;
    public bool IsValueCreated => true;
}