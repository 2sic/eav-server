using System;
using System.Collections.Generic;
using ToSic.Lib.Logging;


namespace ToSic.Eav.Run.Unknown
{
    public class WarnUseOfUnknown<T>
    {
        public WarnUseOfUnknown(History logHistory)
        {
            var usingType = typeof(T);

            if (AlreadyWarnedTypes.Contains(usingType)) return;
            AlreadyWarnedTypes.Add(usingType);

            var log = new Log($"{LogNames.NotImplemented}.Warn", initialMessage: $"Warning Mock Implementation of {usingType.FullName}");
            log.A(
                "The system uses extensive Dependency Injection, and defaults to 'Unknown' implementations if something is missing. ");
            log.A("You are seeing this warning, because apparently such an implementation was initialized, which shouldn't happen in a good productive environment.");
            log.A("This warning will only appear once per used type");
            log.A("See https://r.2sxc.org/unknown-implementations");
            logHistory.Add("warnings-not-implemented", log);
        }

        private static readonly List<Type> AlreadyWarnedTypes = new List<Type>();
    }
}
