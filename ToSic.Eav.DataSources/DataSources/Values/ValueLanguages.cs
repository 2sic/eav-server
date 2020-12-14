using System;
using ToSic.Eav.Context;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;

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
        internal const string LanguageDefault = "default";
        internal const string LanguageCurrent = "current";

        internal static string LanguageSettingsPlaceholder = $"[Settings:{LangKey}||{LanguageDefault}]";


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
                    ? new string[0]
                    : new[] { resolved });


            wrapLog($"Error - can't figure out '{lang}'", null);
            throw new Exception($"Can't figure out what language to use: '{lang}'. Expected '{LanguageDefault}', '{LanguageCurrent}' or a 2-character code");
        }

        private string ResolveOneLanguageCode(string lang, ILog log)
        {
            var wrapLog = log.Call<string>(lang);

            if (string.IsNullOrWhiteSpace(lang) || lang == LanguageDefault)
                return wrapLog(LanguageDefault, string.Empty); // empty string / no language means the default language

            if (lang == LanguageCurrent)
                return wrapLog(LanguageCurrent, _cultureResolver.SafeCurrentCultureCode());

            if (lang.Length == 2 || (lang.Length == 5 && lang.Contains("-")))
                return wrapLog("Exact" + lang, lang);

            return null;
        }
    }
}
