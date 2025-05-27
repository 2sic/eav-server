using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using ToSic.Lib.Data;
using ToSic.Lib.Services;

namespace ToSic.Sys.Startup;

/// <summary>
/// A template object to build a global catalog of features or something.
/// Goal is that it should be implemented by a specific class, 
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class GlobalCatalogBase<T>: ServiceBase, ILogShouldNeverConnect where T : IHasIdentityNameId
{
#pragma warning disable CS8618, CS9264
    protected GlobalCatalogBase(ILogStore logStore, string logName, CodeRef code) : base(logName)
#pragma warning restore CS8618, CS9264
    {
        logStore.Add(LogNames.LogStoreStartUp, Log);
        // ReSharper disable ExplicitCallerInfoArgument
        Log
            .FnCode(message: $"Catalog Created for {typeof(T).Name}", timer: true, code: code)
            .Done();
        // ReSharper restore ExplicitCallerInfoArgument
    }

    /// <summary>
    /// The dictionary containing all the items.
    /// Case-insensitive.
    /// </summary>
    [field: System.Diagnostics.CodeAnalysis.AllowNull, System.Diagnostics.CodeAnalysis.MaybeNull]
    public IReadOnlyDictionary<string, T> Dictionary
    {
        get => field ??= new ReadOnlyDictionary<string, T>(_master);
        private set;
    }

    [field: System.Diagnostics.CodeAnalysis.AllowNull, System.Diagnostics.CodeAnalysis.MaybeNull]
    public IReadOnlyCollection<T> List
    {
        get => field ??= new ReadOnlyCollection<T>(_master.Values.ToList());
        private set;
    }

    public virtual T? TryGet(string name)
        => Dictionary.TryGetValue(name, out var value) ? value : default;

    /// <summary>
    /// Add things to the registry
    /// </summary>
    /// <param name="items"></param>
    public void Register(params T?[] items)
    {
        var l = Log.Fn($"Will add {items.Length} items");
        // add all features if it doesn't yet exist, otherwise update
        foreach (var f in items)
            if (f != null)
            {
                l.A($"Adding {f.NameId}");
                _master.AddOrUpdate(f.NameId, f, (_, _) => f);
            }

        // Reset the read-only dictionary
        Dictionary = null!;
        List = null!;
        l.Done($"now contains {_master.Count} items");
    }

    private readonly ConcurrentDictionary<string, T> _master = new(StringComparer.InvariantCultureIgnoreCase);
}