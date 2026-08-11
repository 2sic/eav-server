namespace ToSic.Sys;

/// <summary>
/// A <see cref="LazyGet{TValue}"/> with the ability to reset or flush the value.
/// </summary>
/// <typeparam name="TValue"></typeparam>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LazyGetAndReset<TValue>: LazyGet<TValue>
{
    /// <summary>
    /// Reset the state and value so it will be re-generated next time it's needed.
    /// </summary>
    public void Reset()
        => IsValueCreated = false;
    
    /// <summary>
    /// Reset the state and value so it will be re-generated next time it's needed.
    /// </summary>
    public void Set(TValue newValue)
    {
        Value = newValue;
        IsValueCreated = true;
    }
}