﻿using System.Runtime.CompilerServices;

namespace ToSic.Lib.Logging;

// ReSharper disable once InconsistentNaming
partial class ILog_Add
{
    /// <summary>
    /// Add a **Exception** to the log.
    /// </summary>
    /// <param name="log">The log object (or null)</param>
    /// <param name="exception">The exception object.</param>
    /// <param name="cPath">Code file path, auto-added by compiler</param>
    /// <param name="cName">Code method name, auto-added by compiler</param>
    /// <param name="cLine">Code line number, auto-added by compiler</param>
    /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    [return: NotNullIfNotNull(nameof(exception))]
    public static TException? Ex<TException>(this ILog? log,
        TException? exception,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default
    ) where TException : Exception
        => log.ExceptionInternal(exception, CodeRef.Create(cPath!, cName!, cLine));

    /// <summary>
    /// Add a **Exception** to the log.
    /// </summary>
    /// <param name="log">The log object (or null)</param>
    /// <param name="message">message to also add</param>
    /// <param name="exception">The exception object.</param>
    /// <param name="cPath">Code file path, auto-added by compiler</param>
    /// <param name="cName">Code method name, auto-added by compiler</param>
    /// <param name="cLine">Code line number, auto-added by compiler</param>
    /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static TException Ex<TException>(this ILog? log,
        string message,
        TException exception,
        [CallerFilePath] string? cPath = default,
        [CallerMemberName] string? cName = default,
        [CallerLineNumber] int cLine = default
    ) where TException : Exception
    {
        log.E(message);
        return log.ExceptionInternal(exception, CodeRef.Create(cPath!, cName!, cLine));
    }
}