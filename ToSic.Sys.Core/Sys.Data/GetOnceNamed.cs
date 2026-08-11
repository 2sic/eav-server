using System.Collections.Concurrent;
using static System.StringComparer;

namespace ToSic.Sys.Data;

[InternalApi_DoNotUse_MayChangeWithoutNotice("Experimental")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class GetOnceNamed<TResult>
{
    private IDictionary<string, TResult> _cache = new ConcurrentDictionary<string, TResult>(InvariantCultureIgnoreCase);

    /// <summary>
    /// Construct an empty GetOnceNamed object for use later on.
    ///
    /// In case you're wondering why we can't pass the generator in on the constructor:
    /// Reason is that in most cases we need real objects in the generator function,
    /// which doesn't work in a `static` context.
    /// This means that if the = new LazyGet() is run on the private property
    /// (which is the most common case) most generators can't be added. 
    /// </summary>
    public GetOnceNamed() { }

    /// <summary>
    /// Get the value. If not yet retrieved, use the generator function (but only once). 
    /// </summary>
    /// <param name="name">Name of named instance, to use for caching</param>
    /// <param name="generator">Function which will generate the value on first use.</param>
    /// <returns></returns>
    public TResult Get(string name, Func<TResult> generator)
    {
        if (_cache.TryGetValue(name, out var result))
            return result;
        
        // Important: don't use try/catch, because the parent should be able to decide if try/catch is appropriate
        var value = generator();
        
        _cache.Add(name, value);
        // Important: This must happen explicitly after the generator() - otherwise there is a risk of cyclic code which already assume
        // the value was created, while still inside the creation of the value.
        // So we would rather have a stack overflow and find the problem code, then to let the code assume the value was already made and null
        return value;
    }

    public void Reset()
        => _cache = new ConcurrentDictionary<string, TResult>();

    public void Reset(string name)
        => _cache.Remove(name);

    public bool IsValueCreated(string name)
        => _cache.ContainsKey(name);

}