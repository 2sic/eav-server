using System;
using System.Collections.Generic;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Obsolete
{
    /// <summary>
    /// Special logger to log obsolete messages
    /// </summary>
    public class LogObsolete: HasLog
    {
        public const string ObsoleteNameInHistory = "warning-obsolete";
        public const int MaxGeneralToLog = 25;
        public const int MaxSpecificToLog = 1;

        public LogObsolete(string obsoleteId, string caseIdentifier, string since, string till, string link, Action<ILog> addMore) : base(LogNames.Eav + ".Obsolet")
        {
            try
            {
                // Count how often this case has already been logged
                ObsoleteIdCount.TryGetValue(obsoleteId, out var countGeneral);
                ObsoleteIdCount[obsoleteId] = ++countGeneral;

                var countSpecific = 0;
                var longId = obsoleteId + ":" + caseIdentifier;
                if (caseIdentifier.HasValue())
                {
                    // Count how often this case has already been logged
                    SpecificCases.TryGetValue(longId, out countSpecific);
                    SpecificCases[longId] = ++countSpecific;
                }

                // Don't log to normal warnings if it's been reported already
                if (countGeneral > MaxGeneralToLog || countSpecific > MaxSpecificToLog) return;

                Log.Add($"Obsolete: {longId} is deprecated since '{since}' and will be removed in '{till}'");
                Log.Add($"For further information, check: {link}");

                try
                {
                    if (addMore != null)
                    {
                        Log.Add("Additional Info:");
                        addMore.Invoke(Log);
                    }
                }
                catch (Exception ex)
                {
                    Log.Add("Error logging additional info.");
                    Log.Exception(ex);
                }

                // Stop logging a general case if it's already been logged very often
                if (countGeneral == MaxGeneralToLog)
                    Log.Add(
                        $"This is the last log we'll add for the case '{obsoleteId}', further messages won't be logged for this problem.");

                // Stop logging a specific case if it has already been logged
                if (countSpecific == MaxSpecificToLog)
                    Log.Add(
                        $"This is the only log we'll add for the id '{longId}', further messages won't be logged for this.");


                var history = new LogHistory();
                history.ForceAdd(ObsoleteNameInHistory, Log);
            }
            catch
            {
                /* ignore - avoid throwing errors just because logging is defect */
            }
        }

        private static readonly Dictionary<string, int> ObsoleteIdCount = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        private static Dictionary<string, int> SpecificCases =
            new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
    }
}
