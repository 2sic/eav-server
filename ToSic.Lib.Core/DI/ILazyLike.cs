namespace ToSic.Lib.DI;

/// <summary>
/// This should ensure that naming of objects which do special DI kind of stuff should have the same naming as Lazy...
/// </summary>
/// <typeparam name="T"></typeparam>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ILazyLike<out T>
{
    T Value { get; }

    bool IsValueCreated { get; }
}