namespace ToSic.Eav.Logging.Call
{
    public class LogCall<T>: LogCallBase
    {
        
        internal LogCall(ILog log, CodeRef code, bool isProp, string parameters = null, string message = null, bool startTimer = false) 
            : base(log, code, isProp, parameters, message, startTimer)
        {
        }

        public T Return(T result)
        {
            DoneInternal(null);
            return result;
        }

        public T Return(T result, string message)
        {
            DoneInternal(message);
            return result;
        }

        public T ReturnAndLog(T result)
        {
            DoneInternal($"{result}");
            return result;
        }

        public T ReturnNull()
        {
            DoneInternal(null);
            return default;
        }
        public T ReturnNull(string message)
        {
            DoneInternal(message);
            return default;
        }

    }
}
