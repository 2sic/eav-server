using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ToSic.Lib.Logging;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Obsolete
{
    /// <summary>
    /// Special logger to log obsolete messages
    /// </summary>
    public class LogObsolete: HasLog
    {
        public const string ObsoleteNameInHistory = LogConstants.HistoryWarningsPrefix + "obsolete";
        public const int MaxGeneralToLog = 25;
        public const int MaxSpecificToLog = 1;
        private const string MainError = "error";

        public LogObsolete(string obsoleteId, string caseIdentifier, string since, string till, string link, Action<ILog> addMore) : base(EavLogs.Eav + ".Obsolt")
        {
            try
            {
                var stackInfo = GetStackAndMainFile();

                // If we don't have a case-identifier, use the path of the top CSHTML as the identifier
                if (string.IsNullOrWhiteSpace(caseIdentifier) && stackInfo.Main != MainError)
                    caseIdentifier = stackInfo.Main;

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

                var msg = $"Obsolete: {longId} is deprecated since '{since}' "
                          + (till == null ? "and has been removed." : $"and will be removed in {till}.");
                Log.A(msg);
                Log.A($"For further information, check: {link}");

                try
                {
                    if (addMore != null)
                    {
                        Log.A("Additional Info:");
                        addMore.Invoke(Log);
                    }
                    else
                        Log.A("No additional info.");
                }
                catch (Exception ex)
                {
                    Log.A("Error logging additional info.");
                    Log.Ex(ex);
                }

                // Try to add the stack trace
                try
                {
                    Log.A("Will now list Razor files in StackTrace");
                    foreach (var razorFile in stackInfo.AllCshtml)
                        Log.A(razorFile);

                    Log.A("Entire Stack for additional debugging");
                    Log.A(stackInfo.Stack);
                }
                catch (Exception ex)
                {
                    Log.A("Error logging stack trace.");
                    Log.Ex(ex);
                }

                // Stop logging a general case if it's already been logged very often
                if (countGeneral == MaxGeneralToLog)
                    Log.A($"This is the last log we'll add for the case '{obsoleteId}', further messages won't be logged for this problem.");

                // Stop logging a specific case if it has already been logged
                if (countSpecific == MaxSpecificToLog)
                    Log.A($"This is the only log we'll add for the id '{longId}', further messages won't be logged for this.");


                new LogStoreLive().ForceAdd(ObsoleteNameInHistory, Log);
            }
            catch
            {
                /* ignore - avoid throwing errors just because logging is defect */
            }
        }

        private (string Stack, string Main, string[] AllCshtml) GetStackAndMainFile()
        {
            var stack = Environment.StackTrace;
            try
            {
                // Note: in the stack, all relevant entries start with "Execute() in " and the path to the .cshtml
                var razorFiles = Regex.Matches(stack, @"Execute\(\).*?\.cshtml");
                var all = razorFiles
                    .Cast<Match>()
                    .Select(m => m.Value.Replace("Execute() in ", ""))
                    .ToArray();
                return (stack, all.FirstOrDefault(), all);
            }
            catch
            {
                return (stack, MainError, Array.Empty<string>());
            }
        }

        private static readonly Dictionary<string, int> ObsoleteIdCount = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        private static Dictionary<string, int> SpecificCases =
            new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
    }
}
