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
    public class LogCall: LogCallBase, ILogCall
    {
        [PrivateApi]
        internal LogCall(ILog log, CodeRef code, bool isProperty, string parameters = null, string message = null, bool timer = false)
            : base(log, code, isProperty, parameters, message, timer)
        {
        }
    }
}
