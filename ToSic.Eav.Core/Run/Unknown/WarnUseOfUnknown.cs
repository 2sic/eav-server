using System;
using System.Collections.Generic;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Run.Unknown
{
    public class WarnUseOfUnknown<T>
    {
        public WarnUseOfUnknown(LogHistory logHistory)
        {
            var usingType = typeof(T);

            if (AlreadyWarnedTypes.Contains(usingType)) return;
            AlreadyWarnedTypes.Add(usingType);

            var log = new Log($"{LogNames.NotImplemented}.Warn", initialMessage: $"Warning Mock Implementation of {usingType.FullName}");
            log.Add(
                "The system uses extensive Dependency Injection, and defaults to 'Unknown' implementations if something is missing. ");
            log.Add("You are seeing this warning, because apparently such an implementation was initialized, which shouldn't happen in a good productive environment.");
            log.Add("This warning will only appear once per used type");
            logHistory.Add("warnings-not-implemented", log);
        }

        private static readonly List<Type> AlreadyWarnedTypes = new List<Type>();
    }
}
