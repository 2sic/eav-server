using System;
using ToSic.Eav.Logging;

namespace ToSic.Eav.WebApi.Sys
{
    // Release routes

    public class LogControllerReal: HasLog<LogControllerReal>
    {
        public const string LogSuffix = "Log";

        public LogControllerReal() : base($"{LogNames.WebApi}.{LogSuffix}Rl") { }

        public string EnableDebug(Func<int, string> activateForDuration, int duration = 1)
        {
            Log.Add("Extended logging will set for duration:" + duration);
            var msg = activateForDuration(duration);
            Log.Add(msg);
            return msg;
        }
    }
}
