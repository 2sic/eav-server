namespace ToSic.Eav.Logging
{
    public class LogCall<T>: LogCallBase
    {
        
        internal LogCall(ILog log, CodeRef code, bool isProp, string parameters = null, string message = null, bool startTimer = false) 
            : base(log, code, isProp, parameters, message, startTimer)
        {
        }

        public T Return(T result) => this.Return(result, null);

        public T Return(T result, string message)
        {
            this.DoneInternal(message);
            return result;
        }

        public T ReturnAndLog(T result) => this.Return(result, $"{result}");

        public T ReturnNull() => this.Return(default, null);

        public T ReturnNull(string message) => this.Return(default, message);
    }
}
