﻿namespace ToSic.Eav.Logging.Call
{
    public class LogCall<T>: LogCallBase
    {
        
        internal LogCall(ILog log, CodeRef code, bool isProp, string parameters = null, string message = null, bool startTimer = false) : base(log, code, isProp, parameters, message, startTimer)
        {
        }

        public T Done(string message, T result)
        {
            DoneInternal(message);
            return result;
        }

    }
}