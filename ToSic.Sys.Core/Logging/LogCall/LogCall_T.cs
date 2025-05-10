namespace ToSic.Lib.Logging;

/// <summary>
/// A mini logger for a function call, which should be closed using a form of `Return(...)` when the function completes.
/// </summary>
/// <remarks>
/// 1. It's important to note that many Return commands are extension methods.
/// 1. Certain types such as `bool` have their own custom `Return...` commands, such as `ReturnFalse()`
/// </remarks>
/// <typeparam name="T">Type of data to return at the end of the call.</typeparam>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class LogCall<T>: LogCallBase, ILogCall<T>
{
    [PrivateApi]
    internal LogCall(ILog? log, CodeRef code, bool isProperty, string? parameters = default, string? message = default, bool timer = false) 
        : base(log, code, isProperty, parameters, message, timer)
    { }
}