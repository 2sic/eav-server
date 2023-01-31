using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// A Log Call which doesn't expect to return a value.
    /// Usually for `void` methods.
    /// </summary>
    /// <remarks>
    /// Note that normal code will never create this object, but get such an object when calling `ILog.Fn(...)`
    /// </remarks>
    [PrivateApi]
    public class LogCall: LogCallBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="log">The Log or a Call-Log, will be unwrapped automatically</param>
        /// <param name="code"></param>
        /// <param name="isProperty"></param>
        /// <param name="parameters"></param>
        /// <param name="message"></param>
        /// <param name="timer"></param>
        [PrivateApi]
        internal LogCall(ILog log, CodeRef code, bool isProperty, string parameters = null, string message = null, bool timer = false)
            : base(log, code, isProperty, parameters, message, timer)
        {
        }
    }
}
