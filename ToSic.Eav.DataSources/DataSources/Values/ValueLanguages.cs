using System;
using ToSic.Eav.Context;
using ToSic.Eav.Logging;

namespace ToSic.Eav.DataSources
{
    public class ValueLanguages: HasLog<ValueLanguages>
    {
        #region Constructor / DI

        public ValueLanguages(IZoneCultureResolver cultureResolver): base("Ds.ValLng")
        {
            _cultureResolver = cultureResolver;
        }
        private readonly IZoneCultureResolver _cultureResolver;

        #endregion

        /// <summary>
        /// Key to be used in settings etc.
        /// </summary>
        internal const string LangKey = "Languages";

        /// <summary>
        /// Constants for certain resolution modes
        /// </summary>
        internal const string LanguageDefaultPlaceholder = "default";
        internal const string LanguageCurrentPlaceholder = "current";

        internal static string LanguageSettingsPlaceholder = $"[Settings:{LangKey}||{LanguageDefaultPlaceholder}]";


        /// <summary>
        /// Prepare language list to use in lookup
        /// </summary>
        /// <returns></returns>
        internal string[] PrepareLanguageList(string languages, ILog log)
        {
            var lang = languages.ToLowerInvariant().Trim();

            var wrapLog = log.Call<string[]>(lang);

            var resolved = ResolveOneLanguageCode(lang, log);

            // if null, not ok - continue to error
            // if String.Empty, then we just want the default, so use empty array (faster)
            if (resolved != null)
                return wrapLog(resolved, resolved == string.Empty
                    ? Array.Empty<string>()
                    : new[] { resolved });


            wrapLog($"Error - can't figure out '{lang}'", null);
            throw new Exception($"Can't figure out what language to use: '{lang}'. Expected '{LanguageDefaultPlaceholder}', '{LanguageCurrentPlaceholder}' or a 2-character code");
        }

        private string ResolveOneLanguageCode(string lang, ILog log)
        {
            var wrapLog = log.Call<string>(lang);

            if (string.IsNullOrWhiteSpace(lang) || lang == LanguageDefaultPlaceholder)
                return wrapLog(LanguageDefaultPlaceholder, string.Empty); // empty string / no language means the default language

            if (lang == LanguageCurrentPlaceholder)
                return wrapLog(LanguageCurrentPlaceholder, _cultureResolver.SafeCurrentCultureCode());

            if (lang.Length == 2 || lang.Length == 5 && lang.Contains("-"))
                return wrapLog("Exact" + lang, lang);

            return null;
        }
    }
}
