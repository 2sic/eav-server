using ToSic.Lib.Documentation;

namespace ToSic.Lib.Data;

/// <summary>
/// Helper base class for all wrappers
/// </summary>
/// <typeparam name="T"></typeparam>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
[PrivateApi]
public abstract class Wrapper<T>: IWrapper<T>
{
    private T _unwrappedContents;

    /// <inheritdoc />
    public virtual T GetContents() => _unwrappedContents;

    protected Wrapper(T contents) => _unwrappedContents = contents;


    protected void Wrap(T contents) => _unwrappedContents = contents;
}