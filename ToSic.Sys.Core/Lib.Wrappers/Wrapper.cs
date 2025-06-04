namespace ToSic.Lib.Wrappers;

/// <summary>
/// Helper base class for all wrappers
/// </summary>
/// <typeparam name="T"></typeparam>
[ShowApiWhenReleased(ShowApiMode.Never)]
[PrivateApi]
public abstract class Wrapper<T>(T? unwrappedContents) : IWrapper<T>
{
    private T? _unwrappedContents = unwrappedContents;

    /// <inheritdoc />
    public virtual T? GetContents() => _unwrappedContents;


    protected void Wrap(T? contents) => _unwrappedContents = contents;
}