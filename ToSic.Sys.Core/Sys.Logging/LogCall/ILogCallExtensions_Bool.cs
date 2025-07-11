﻿namespace ToSic.Sys.Logging;

// ReSharper disable once InconsistentNaming
partial class ILogCallExtensions
{
    /// <summary>
    /// Return `true` for `ILogCall bool` objects.
    /// </summary>
    /// <param name="logCall">The log call or null</param>
    /// <returns></returns>
    public static bool ReturnTrue(this ILogCall<bool>? logCall)
        => logCall?.ReturnTrue("") ?? true;

    /// <summary>
    /// Return `true` for `ILogCall bool` objects with a message.
    /// </summary>
    /// <param name="logCall">The log call or null</param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static bool ReturnTrue(this ILogCall<bool>? logCall, string message)
        => logCall?.Return(true, $"{true} {message}") ?? true;

    /// <summary>
    /// Return `false` for `ILogCall bool` objects.
    /// </summary>
    /// <param name="logCall">The log call or null</param>
    /// <returns></returns>
    public static bool ReturnFalse(this ILogCall<bool>? logCall)
        => logCall?.ReturnFalse("") ?? false;

    /// <summary>
    /// Return `false` for `ILogCall bool` objects with a message.
    /// </summary>
    /// <param name="logCall">The log call or null</param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static bool ReturnFalse(this ILogCall<bool>? logCall, string message)
        => logCall?.Return(false, $"{false} {message}") ?? false;

}