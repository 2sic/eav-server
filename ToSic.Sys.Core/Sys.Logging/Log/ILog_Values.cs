using System.Globalization;
using System.Runtime.CompilerServices;

namespace ToSic.Sys.Logging;

/// <summary>
/// Various extensions for <see cref="ILog"/> objects to add logs.
/// They are all implemented as extension methods, so that they will not fail even if the log object is null.
/// </summary>
[PublicApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
// ReSharper disable once InconsistentNaming
public static class ILog_Values
{
    /// <summary>
    /// Handle a value, and possibly do something with it.
    /// </summary>
    /// <param name="log">The log object (or null)</param>
    /// <param name="value"></param>
    /// <param name="ifTrue"></param>
    /// <param name="ifFalse"></param>
    /// <param name="cPath">Code file path, auto-added by compiler</param>
    /// <param name="cName">Code method name, auto-added by compiler</param>
    /// <param name="cLine">Code line number, auto-added by compiler</param>
    /// <param name="options"></param>
    /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool Bool(this ILog? log,
        bool value,
        string? ifTrue = null,
        string? ifFalse = null,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default,
        EntryOptions? options = default
    )
    {
        var msg = value ? ifTrue : ifFalse;
        if (msg != null)
            log.AddInternal(msg, CodeRef.Create(cPath!, cName!, cLine), options);
        return value;
    }
}