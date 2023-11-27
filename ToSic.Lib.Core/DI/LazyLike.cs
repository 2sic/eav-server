namespace ToSic.Lib.DI;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LazyLike<TService>: ILazyLike<TService>
{
    public LazyLike(TService value) => Value = value;
    public TService Value { get; }
    public bool IsValueCreated => true;
}