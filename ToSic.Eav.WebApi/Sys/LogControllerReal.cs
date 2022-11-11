using System;
using ToSic.Lib.Logging;

namespace ToSic.Eav.WebApi.Sys
{
    // Release routes

    public class LogControllerReal: HasLog
    {
        public const string LogSuffix = "Log";

        public LogControllerReal() : base($"{LogNames.WebApi}.{LogSuffix}Rl") { }

        public string EnableDebug(Func<int, string> activateForDuration, int duration = 1)
        {
            Log.A("Extended logging will set for duration:" + duration);
            var msg = activateForDuration(duration);
            Log.A(msg);
            return msg;
        }
    }
}
