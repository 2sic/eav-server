namespace ToSic.Sys;

/// <summary>
/// Simple helper class to use on object properties which should be generated once.
/// Similar to Lazy(), but the generator function is passed in on the Get() call, not on the constructor.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <remarks>
/// Important for properties which can also return null, because then checking for null won't work to determine if we already tried to retrieve it.
/// 
/// Constructor is empty.
///
/// In case you're wondering why we can't pass the generator in on the constructor:
/// Reason is that in most cases we need real objects in the generator function,
/// which doesn't work in a `static` context.
/// This means that if the = new LazyGet() is run on the private property
/// (which is the most common case) most generators can't be added.
///
/// Typical use cases:
/// 
/// 1. Properties which could validly return a null, in which case ?.Get() would run too often.
/// 2. Properties which must - in rare cases, be reset
/// 3. Properties which must check if they were already created (like Lazy())
/// 4. Properties which have non-nullable types like tuples, which can't self-detect if they are still empty
/// 5. Methods which behave like properties but as methods (typically to prevent serialization), so they don't have a backing `field`, needing additional code
/// </remarks>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class LazyGet<TValue>
{
    /// <summary>
    /// Get the value. If not yet retrieved, use the generator function (but only once). 
    /// </summary>
    /// <param name="generator">Function which will generate the value on first use.</param>
    /// <returns></returns>
    public TValue? Get(Func<TValue> generator)
    {
        if (IsValueCreated)
            return Value;
        // Important: don't use try/catch, because the parent should be able to decide if try/catch is appropriate
        Value = generator();
        // Important: This must happen explicitly after the generator() - otherwise there is a risk of cyclic code which already assume
        // the value was created, while still inside the creation of the value.
        // So we would rather have a stack overflow and find the problem code, then to let the code assume the value was already made and null
        IsValueCreated = true;
        return Value;
    }
    
    /// <summary>
    /// Determines if value has been created.
    /// The name `IsValueCreated` is the same as in a Lazy() object
    /// </summary>
    public bool IsValueCreated { get; protected internal set; }
    protected internal TValue? Value;
}