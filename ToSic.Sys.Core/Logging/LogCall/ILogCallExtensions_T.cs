namespace ToSic.Lib.Logging;

// ReSharper disable once InconsistentNaming
partial class ILogCallExtensions
{
    /// <summary>
    /// Close the log call and return a specific result, without adding any message.
    /// </summary>
    /// <param name="logCall">The log call or null</param>
    /// <param name="result">The result to return</param>
    /// <returns>The result specified</returns>
    public static T Return<T>(this ILogCall<T> logCall, T result) => logCall.Return(result, null);

    /// <summary>
    /// Close the log call and return a specific result, without adding any message.
    /// </summary>
    /// <param name="logCall">The log call or null</param>
    /// <param name="result">The result to return</param>
    /// <param name="message">Message to add to the log</param>
    /// <returns>The result specified</returns>
    public static T Return<T>(this ILogCall<T> logCall, T result, string message)
    {
        logCall?.DoneInternal(message);
        return result;
    }


    /// <summary>
    /// Return a value with the standard message "ok"
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logCall">The log call or null</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static T ReturnAsOk<T>(this ILogCall<T> logCall, T result) => logCall.Return(result, "ok");

    /// <summary>
    /// Return a value with the standard message "error"
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="logCall">The log call or null</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static T ReturnAsError<T>(this ILogCall<T> logCall, T result, string message = default)
        => logCall.Return(result, $"error {message}");

    /// <summary>
    /// Return a value and log the result as well.
    /// Note that if the object supports <see cref="ICanDump"/> it will use that dump to log it's value.
    /// </summary>
    /// <param name="logCall">The log call or null</param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static T ReturnAndLog<T>(this ILogCall<T> logCall, T result) =>
        logCall.Return(result, (result as ICanDump)?.Dump() ?? $"{result}");
    public static T ReturnAndLogIfNull<T>(this ILogCall<T> logCall, T result) =>
        logCall.Return(result, (result as ICanDump)?.Dump() ?? $"is null: {result == null}");

    /// <summary>
    /// Return a value and log the result as well + add a message.
    /// Note that if the object supports <see cref="ICanDump"/> it will use that dump to log it's value.
    /// </summary>
    /// <param name="logCall">The log call or null</param>
    /// <param name="result"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static T ReturnAndLog<T>(this ILogCall<T> logCall, T result, string message)
        => logCall.Return(result, $"{(result as ICanDump)?.Dump() ?? $"{result}"} - {message}");


    /// <summary>
    /// Return a null or the default value (like a zero for int) without further messages.
    /// </summary>
    /// <param name="logCall">The log call or null</param>
    /// <returns></returns>
    public static T ReturnNull<T>(this ILogCall<T> logCall) => logCall.Return(default, "null");

    /// <summary>
    /// Return a null or the default value (like a zero for int) with specified message.
    /// </summary>
    /// <param name="logCall">The log call or null</param>
    /// <param name="message">Message to add to the log</param>
    /// <returns></returns>
    public static T ReturnNull<T>(this ILogCall<T> logCall, string message) => logCall.Return(default, message);

    #region String

    /// <summary>
    /// Return `true` for `ILogCall bool` objects.
    /// </summary>
    /// <param name="logCall">The log call or null</param>
    /// <returns></returns>
    public static string ReturnEmpty(this ILogCall<string> logCall) => logCall.Return("", "empty");

    /// <summary>
    /// Return `true` for `ILogCall bool` objects.
    /// </summary>
    /// <param name="logCall">The log call or null</param>
    /// <param name="message">Message to add to the log</param>
    /// <returns></returns>
    public static string ReturnEmpty(this ILogCall<string> logCall, string message) => logCall.Return("", message);

    #endregion
}