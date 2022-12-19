namespace ToSic.Lib.Logging
{
    public class LogCall<T>: LogCallBase
    {
        internal LogCall(ILog log, CodeRef code, bool isProperty, string parameters = null, string message = null, bool startTimer = false) 
            : base(log, code, isProperty, parameters, message, startTimer)
        {
        }

        public T Return(T result) => Return(result, null);

        public T Return(T result, string message)
        {
            this.DoneInternal(message);
            return result;
        }

        public T ReturnAndLog(T result) => Return(result, (result as ICanDump)?.Dump() ?? $"{result}");

        public T ReturnAndLog(T result, string message) 
            => Return(result, $"{(result as ICanDump)?.Dump() ?? $"{result}"} - {message}");

        public T ReturnNull() => Return(default, "null");

        public T ReturnNull(string message) => Return(default, message);
    }
}
