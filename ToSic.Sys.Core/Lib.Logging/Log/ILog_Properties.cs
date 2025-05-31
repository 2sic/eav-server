using System.Runtime.CompilerServices;
using static ToSic.Lib.Logging.CodeRef;

namespace ToSic.Lib.Logging;

/// <summary>
/// Extension methods for property getters and setters.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("Still experimental")]
// ReSharper disable once InconsistentNaming
[ShowApiWhenReleased(ShowApiMode.Never)]
// ReSharper disable once InconsistentNaming
public static class ILog_Properties
{
    internal const string GetPrefix = "get:";
    internal const string SetPrefix = "set:";

    /// <summary>
    /// Short wrapper for PropertyGet calls which only return the value.
    /// </summary>
    /// <typeparam name="TProperty">Type of return value</typeparam>
    /// <returns></returns>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("2dm: Experimental, don't use yet")]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static TProperty Getter<TProperty>(this ILog log,
        Func<TProperty> getter,
        bool timer = default,
        bool enabled = true,
        string? message = default,
        string? parameters = default,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default
    )
    {
        var l = enabled
            ? new LogCall<TProperty>(log, Create(cPath!, $"{GetPrefix}{cName}", cLine), true, parameters, message, timer)
            : null;
        var result = getter();
        return !enabled ? result : l.ReturnAndLog(result);
    }
}