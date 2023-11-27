using System;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging;

/// <summary>
/// Extensions for <see cref="ILogCall"/> objects which don't need to return a value.
/// </summary>
[PublicApi]
// ReSharper disable once InconsistentNaming
public static partial class ILogCallExtensions
{
    /// <summary>
    /// Complete/close an <see cref="ILogCall"/> without returning a value.
    /// </summary>
    /// <param name="logCall">The log call or null</param>
    public static void Done(this ILogCall logCall) => logCall.DoneInternal(null);

    /// <summary>
    /// Complete/close an <see cref="ILogCall"/> without returning a value, and also add a message.
    /// </summary>
    /// <param name="logCall">The log call or null</param>
    /// <param name="message"></param>
    public static void Done(this ILogCall logCall, string message) => logCall.DoneInternal(message);

    /// <summary>
    /// Complete/close an <see cref="ILogCall"/> without returning a value, and also add a message.
    /// </summary>
    /// <param name="logCall">The log call or null</param>
    /// <param name="exception"></param>
    public static T Done<T>(this ILogCall logCall, T exception) where T : Exception
    {
        logCall.Ex(exception);
        logCall.DoneInternal("error");
        return exception;
    }
}