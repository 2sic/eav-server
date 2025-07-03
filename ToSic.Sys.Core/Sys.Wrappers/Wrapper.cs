namespace ToSic.Sys.Wrappers;

/// <summary>
/// Helper base class for all wrappers
/// </summary>
/// <typeparam name="T"></typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class Wrapper<T>(T? unwrappedContents) : IWrapper<T>
{
    private T? _unwrappedContents = unwrappedContents;

    /// <inheritdoc />
    public virtual T? GetContents() => _unwrappedContents;


    protected void Wrap(T? contents) => _unwrappedContents = contents;
}