namespace ToSic.Lib.Logging
{
    public class LogCall: LogCallBase
    {
        internal LogCall(ILog log, CodeRef code, bool isProp, string parameters = null, string message = null, bool startTimer = false) : base(log, code, isProp, parameters, message, startTimer)
        {
        }
    }
}
