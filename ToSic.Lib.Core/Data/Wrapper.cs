namespace ToSic.Lib.Data;

/// <summary>
/// Helper base class for all wrappers
/// </summary>
/// <typeparam name="T"></typeparam>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
[PrivateApi]
public abstract class Wrapper<T>(T unwrappedContents) : IWrapper<T>
{
    /// <inheritdoc />
    public virtual T GetContents() => unwrappedContents;


    protected void Wrap(T contents) => unwrappedContents = contents;
}