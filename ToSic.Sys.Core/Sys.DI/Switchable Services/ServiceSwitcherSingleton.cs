namespace ToSic.Sys.DI;

/// <summary>
/// Similar to the <see cref="ServiceSwitcher{T}"/> but special.
/// </summary>
/// <remarks>
/// It will behave as singleton, but will resolve Transient! this is important.
/// Reason is that this way we don't keep a list of all possible services active in memory, just the one that's been selected.
/// </remarks>
/// <typeparam name="T"></typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ServiceSwitcherSingleton<T>(
    ILogStore logStore,
    LazySvc<ServiceSwitcher<T>> serviceSwitcher,
    object[]? connect = default)
    : ServiceBase($"{LogScopes.Lib}.SrvSwS", connect: [.. connect ?? [], logStore, serviceSwitcher]), ILazyLike<T>
    where T : ISwitchableService
{
    public T Value => GetSingletonSwitchableService();

    private T GetSingletonSwitchableService()
    {
        // Already loaded
        if (_preferredService != null)
            return _preferredService;

        logStore.Add(LogNames.LogStoreServiceSwitcher, Log);
        var call = Log.Fn<T>(message: "re-check singleton service");
        // Use a local variable to avoid races with Reset() changing the static field mid-call
        var chosen = serviceSwitcher.Value.Value;
        _preferredService = chosen;
        return call.Return(chosen, $"found {chosen.NameId}");
    }

    /// <summary>
    /// Note: This must be static, as the service itself is used transient, not singleton!
    /// </summary>
    private static T? _preferredService;

    public bool IsValueCreated => _preferredService != null;

    public T? ByNameId(string nameId) => serviceSwitcher.Value.ByNameId(nameId);

    protected void Reset() => _preferredService = default;
}
