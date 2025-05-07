namespace ToSic.Lib.Data;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class WrapperLazy<T>: Wrapper<T> where T : class
{
    public WrapperLazy(T contents) : base(contents)
    {
    }

    /// <summary>
    /// Overload for lazy wrapper.
    /// </summary>
    /// <param name="getContents"></param>
    protected WrapperLazy(Func<T> getContents): base(default) => _getContents = getContents;
    private Func<T> _getContents;

    private T _unwrappedContents;

    protected void Reset() => Wrap(default);

    /// <summary>
    /// Complete the wrapper, ensure the data was retrieved, and drop the getter.
    /// </summary>
    public T Freeze()
    {
        // Reset the contents to ensure final reloading
        Wrap(default);
        // Access the content to ensure loading
        var result = GetContents();
        // Kill the getter
        _getContents = null;
        // return the result
        return result;
    }


    /// <inheritdoc />
    public override T GetContents()
    {
        if (_unwrappedContents != default) return _unwrappedContents;
        _unwrappedContents = base.GetContents() ?? _getContents();
        return _unwrappedContents;
    }
}