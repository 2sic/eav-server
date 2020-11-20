using System;
using System.Threading;
using ToSic.Eav.Logging;

namespace ToSic.Eav.DataSources
{
    internal class ValueLanguages
    {
        /// <summary>
        /// Key to be used in settings etc.
        /// </summary>
        internal const string LangKey = "Languages";

        /// <summary>
        /// Constants for certain resolution modes
        /// </summary>
        internal const string LanguageDefault = "default";
        internal const string LanguageCurrent = "current";

        internal static string LanguageSettingsPlaceholder = $"[Settings:Languages||{LanguageDefault}]";



        /// <summary>
        /// Prepare language list to use in lookup
        /// </summary>
        /// <returns></returns>
        internal static string[] PrepareLanguageList(string languages, ILog log)
        {
            var lang = languages.ToLower().Trim();

            var wrapLog = log.Call<string[]>(lang);

            // check if multiple
            // in this case each position must contain a code or something, empty are filtered out
            // we'll convert each slot, correcting codes like $p etc.
            // Note: 2020-11-17 the EAV doesn't actually support multiple language steps, only 1 + fallback, so 
            // this code is not to be used
            //if (lang.Contains(','))
            //{
            //    Log.Add("Multi-language list detected");
            //    var list = lang.Split(',')
            //        .Select(l => l.Trim())
            //        .Where(l => !string.IsNullOrWhiteSpace(l))
            //        .Select(l => ResolveOneLanguageCode(l, log))
            //        .Where(l => !string.IsNullOrWhiteSpace(l))
            //        .ToArray();
            //    return list;
            //}

            var resolved = ResolveOneLanguageCode(lang, log);

            // if null, not ok - continue to error
            // if String.Empty, then we just want the default, so use empty array (faster)
            if (resolved != null)
                return wrapLog(resolved, resolved == string.Empty
                    ? new string[0]
                    : new[] { resolved });


            wrapLog($"Error - can't figure out '{lang}'", null);
            throw new Exception($"Can't figure out what language to use: '{lang}'. Expected '{ValueLanguages.LanguageDefault}', '{ValueLanguages.LanguageCurrent}' or a 2-character code");
        }

        internal static string ResolveOneLanguageCode(string lang, ILog log)
        {
            var wrapLog = log.Call<string>(lang);

            if (string.IsNullOrWhiteSpace(lang) || lang == ValueLanguages.LanguageDefault)
                return wrapLog(ValueLanguages.LanguageDefault, string.Empty); // empty string / no language means the default language

            if (lang == ValueLanguages.LanguageCurrent)
                return wrapLog(ValueLanguages.LanguageCurrent, Thread.CurrentThread.CurrentCulture.Name);

            if (lang.Length == 2 || (lang.Length == 5 && lang.Contains("-")))
                return wrapLog("Exact" + lang, lang);

            return null;
        }
    }
}
