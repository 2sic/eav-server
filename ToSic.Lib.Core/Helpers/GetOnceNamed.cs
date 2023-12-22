using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ToSic.Lib.Documentation;
using static System.StringComparer;

namespace ToSic.Lib.Helpers;

[InternalApi_DoNotUse_MayChangeWithoutNotice("Experimental")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class GetOnceNamed<TResult>
{
    public IDictionary<string, TResult> Cache = new ConcurrentDictionary<string, TResult>(InvariantCultureIgnoreCase);

    /// <summary>
    /// Construct an empty GetOnceNamed object for use later on.
    ///
    /// In case you're wondering why we can't pass the generator in on the constructor:
    /// Reason is that in most cases we need real objects in the generator function,
    /// which doesn't work in a `static` context.
    /// This means that if the = new GetOnce() is run on the private property
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
        if (Cache.TryGetValue(name, out var result)) return result;
        // Important: don't use try/catch, because the parent should be able to decide if try/catch is appropriate
        var value = generator();
        Cache.Add(name, value);
        // Important: This must happen explicitly after the generator() - otherwise there is a risk of cyclic code which already assume
        // the value was created, while still inside the creation of the value.
        // So we would rather have a stack overflow and find the problem code, than to let the code assume the value was already made and null
        return value;
    }

    public void Reset() => Cache = new ConcurrentDictionary<string, TResult>();

    public void Reset(string name) => Cache.Remove(name);

    public bool IsValueCreated(string name) => Cache.ContainsKey(name);

}