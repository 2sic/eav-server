using System;
using ToSic.Eav.Logging;

namespace ToSic.Eav.WebApi.Sys
{
    // Release routes

    public class LogControllerReal: HasLog<LogControllerReal>
    {
        public const string LogSuffix = "Log";

        public LogControllerReal() : base($"{LogNames.WebApi}.{LogSuffix}Rl") { }

        /// <summary>
        /// Used to be GET System/ExtendedLogging
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="activateForDuration"></param>
        /// <returns></returns>
        public string EnableDebug(Func<int, string> activateForDuration, int duration = 1)
        {
            Log.Add("Extended logging will set for duration:" + duration);
            var msg = activateForDuration(duration);
            Log.Add(msg);
            return msg;
        }
    }
}
