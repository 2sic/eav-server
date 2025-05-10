using System.Diagnostics;

namespace ToSic.Lib.Logging;

/// <summary>
/// A log object used to log the activity of a specific function call.
/// It is usually created in the beginning of the call and closed on various return calls or at the end of the function.
///
/// Note that most of the methods used to complete a call are **extension methods**.
/// </summary>
/// <remarks>
/// Normal code will never create this object, but get such an object when calling `ILog.Fn(...)` extensions <see cref="ILogExtensions"/>.
/// </remarks>
[PublicApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ILogCall: ILog
{
    /// <summary>
    /// The main log-entry for this call, which will also receive the final value at the end of the call.
    /// </summary>
    Entry? Entry { get; }

    /// <summary>
    /// A stopwatch object which is used for timing purposes on this call.
    /// </summary>
    Stopwatch Timer { get; }

    /// <summary>
    /// Reference to the parent log.
    /// </summary>
    ILog? Log { get; }

    //[PrivateApi]
    //bool IsOpen { get; }
}