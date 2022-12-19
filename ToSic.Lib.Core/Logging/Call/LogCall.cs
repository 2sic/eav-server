namespace ToSic.Lib.Logging
{
    public class LogCall: LogCallBase
    {
        internal LogCall(ILog log, CodeRef code, bool isProperty, string parameters = null, string message = null, bool startTimer = false)
            : base(log, code, isProperty, parameters, message, startTimer)
        {
        }
    }
}
