using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// This will parse a line of configuration like this:
    /// Name:de-de=NameDe,fr-fr=NameFr,it-it=NameIt
    /// Description:de-de=DescriptionDe,fr-fr=DescriptionFr,it-it=DescriptionIt
    /// </summary>
    internal class LanguageMap
    {
        /// <summary>
        /// Will load a configuration line and set the error if something doesn't work out
        /// </summary>
        /// <param name="original"></param>
        public LanguageMap(string original) => LoadLine(original);

        public string Original;
        public string Error;
        public string Target;
        public string Source;
        public bool HasLanguages;
        public LanguageToField[] Fields;

        public List<string> FieldNames => _fieldNames ?? (_fieldNames = Fields.Select(f => f.OriginalField).ToList());
        private List<string> _fieldNames;

        public string LoadLine(string original)
        {
            Original = original;
            if (string.IsNullOrWhiteSpace(original))
                return Error = "Error - tried to load an empty configuration line";
            var parts = original.Split(':').Select(s => s.Trim()).ToArray();
            if (parts.Length != 2)
                return Error = "Field Mapping failed, format wrong: each line should contain a [target-field]:[source-field-list] pair.";
            if (string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                return Error = "A part of the assignment x=y is empty";
            Target = parts[0];
            Source = parts[1];

            if (!Source.Contains("=")) return Error = null;
            HasLanguages = true;

            var langMap = Source
                .Split( new []{','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray();
            
            var langPartsError = "";
            Fields = langMap.Select(m =>
                {
                    var langParts = m.Split('=').Select(s => s.Trim()).ToArray();
                    if (langParts.Length != 2)
                    {
                        langPartsError +=
                            "Error: Language parts should have exactly one xx-xx=OldFieldName to separate language from key. ";
                        return null;
                    }

                    if (langParts[0].Length != 5)
                    {
                        langPartsError +=
                            $"Error: language key should be 5 characters - not '{langParts[0]}' ({langParts[0].Length}";
                        return null;
                    }

                    if (string.IsNullOrWhiteSpace(langParts[1]))
                    {
                        langPartsError += "Source field after '=' should be real, but got something empty";
                        return null;
                    }

                    return new LanguageToField
                    {
                        Language = langParts[0],
                        OriginalField = langParts[1],
                    };
                })
                .ToArray();
            if (!string.IsNullOrWhiteSpace(langPartsError))
                return Error = langPartsError;

            return Error = null;
        }
    }

}