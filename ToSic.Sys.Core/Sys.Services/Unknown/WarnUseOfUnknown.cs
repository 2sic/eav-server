namespace ToSic.Sys.Services;

/// <summary>
/// Object which should be used in the DI of Unknown implementations.
/// </summary>
/// <typeparam name="T">The type of the unknown implementation.</typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class WarnUseOfUnknown<T>
{
    /// <summary>
    /// Constructor, which will trigger the warning and add it to the logs.
    /// </summary>
    /// <remarks>
    /// This object will remember that it was already triggered once, so it won't repeat the warning for the same type.
    /// </remarks>
    /// <param name="logStore">The log store to which the warning will be added.</param>
    public WarnUseOfUnknown(ILogStore logStore)
    {
        if (_alreadyWarnedForThisType)
            return;
        _alreadyWarnedForThisType = true;

        var log = new Log($"{LogScopes.NotImplemented}.Warn", message: $"Warning Mock Implementation of {typeof(T).FullName}");
        log.A("The system uses extensive Dependency Injection, and defaults to 'Unknown' implementations if something is missing. ");
        log.A("You are seeing this warning, because apparently such an implementation was initialized, which shouldn't happen in a good productive environment.");
        log.A("This warning will only appear once per used type");
        log.A("See https://go.2sxc.org/unknown-implementations");
        logStore.Add("warnings-not-implemented", log);
    }

    /// <summary>
    /// This will remember that this type (the generic type) was already warning once)
    /// and will not repeat warning again.
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    private static bool _alreadyWarnedForThisType;
}