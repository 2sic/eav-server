using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Persistence
{
    public class EntitySaver
    {
        /// <summary>
        /// Goal: Pass changes into an existing entity so that it can then be saved as a whole, with correct
        /// modifications. 
        /// </summary>
        /// <param name="original"></param>
        /// <param name="update"></param>
        /// <param name="saveOptions"></param>
        /// <returns></returns>
        public static IEntity CreateMergedForSaving(IEntity original, IEntity update, SaveOptions saveOptions)
        {
            #region Step 0: initial error checks
            if(update == null || update.Attributes?.Count == 0)
                throw new Exception("can't prepare entities for saving, no new item with attributes provided");

            var ct = (original ?? update).Type;
            if(ct==null)
                throw new Exception("unknown content-type");

            #endregion

            #region Step 1: check if there is an original item
            // only accept original if it's a real object with a valid GUID, otherwise it's not an existing entity
            bool hasOriginal = !(original == null || (original.EntityId == 0 && original.EntityGuid == Guid.Empty));
            var idProvidingEntity = hasOriginal ? original : update;
            #endregion

            #region Step 2: clean up unwanted attributes from both lists

            var origAttribs = original?.Attributes.Copy();
            var newAttribs = update.Attributes.Copy();

            // Optionally remove original values not in the update
            if (hasOriginal && (!saveOptions.PreserveUntouchedAttributes || !saveOptions.SkipExistingAttributes))
                origAttribs = KeepOnlyKnownKeys(origAttribs, newAttribs.Keys.ToList());

            // Optionaly remove unknown - if possible - of both original and new
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!saveOptions.PreserveUnknownAttributes && ct != null)
            {
                var keys = ct.Attributes.Select(a => a.Name).ToList();
                keys.Add(Constants.EntityFieldGuid);
                keys.Add(Constants.EntityFieldIsPublished);

                if (hasOriginal) origAttribs = KeepOnlyKnownKeys(origAttribs, keys);
                newAttribs = KeepOnlyKnownKeys(newAttribs, keys);
            }

            // optionally remove new things which already exist
            if (hasOriginal && saveOptions.SkipExistingAttributes)
                newAttribs = KeepOnlyKnownKeys(newAttribs, newAttribs.Keys
                    .Where(k => !origAttribs.Keys.Any(
                                ok => string.Equals(k, ok, StringComparison.InvariantCultureIgnoreCase))).ToList());

            #endregion

            #region Step 3: clear unwanted languages as needed

            var hasLanguages = update.GetUsedLanguages().Count + original.GetUsedLanguages().Count > 0;

            // pre check if languages are properly available for clean-up or merge
            if (hasLanguages && !saveOptions.PreserveUnknownLanguages)
                if ((!saveOptions.Languages?.Any() ?? true)
                    || string.IsNullOrWhiteSpace(saveOptions.PrimaryLanguage)
                    || saveOptions.Languages.All(l => l.Key != saveOptions.PrimaryLanguage))
                    throw new Exception("primary language must exist in languages, cannot continue preparation to save with unclear language setup");


            if (hasLanguages && !saveOptions.PreserveUnknownLanguages && (saveOptions.Languages?.Any() ?? false))
            {
                if (hasOriginal) StripUnknownLanguages(origAttribs, saveOptions);
                StripUnknownLanguages(newAttribs, saveOptions);
            }

            #endregion

            // now merge into new target
            Dictionary<string, IAttribute> mergedAttribs = hasOriginal ? origAttribs : newAttribs; // will become 
            if(hasOriginal)
                foreach (var newAttrib in newAttribs)
                    mergedAttribs[newAttrib.Key] = saveOptions.PreserveExistingLanguages && mergedAttribs.ContainsKey(newAttrib.Key)
                        ? MergeAttribute(mergedAttribs[newAttrib.Key], newAttrib.Value, saveOptions)
                        : newAttrib.Value;

            var result = new Entity(idProvidingEntity, mergedAttribs, null);
            ImportKnownProperties(result);
            return result;
        }

        /// <summary>
        /// Will remove all language-information for values which have no language
        /// </summary>
        /// <param name="attribs"></param>
        /// <param name="saveOptions"></param>
        /// <remarks>
        /// this expects that saveOptions contain Languages & PrimaryLanguage, and that this is reliable
        /// </remarks>
        private static void StripUnknownLanguages(Dictionary<string, IAttribute> attribs, SaveOptions saveOptions)
        {
            var languages = saveOptions.Languages;

            foreach (var attribElm in attribs)
            {
                var values = new List<IValue>();       // new empty values list

                // when we go through the values, we should always take the primary language first
                // this is detectable by having either no language, or having the primary language
                var orderedValues = ValuesOrderedForProcessing(attribElm.Value, saveOptions);
                foreach (var value in orderedValues)
                {
                    // create filtered list of languages
                    var newLangs = value.Languages?.Where(l => languages.Any(sysLang => sysLang.Key == l.Key)).ToList();
                    // only keep this value, if it is either the first (so contains primary or null-language) or that it still has a remaining language assignment
                    if (values.Any() && !(newLangs?.Any() ?? false)) continue;
                    value.Languages = newLangs;
                    values.Add(value);
                }
                attribElm.Value.Values = values;
            }
        }

        private static IOrderedEnumerable<IValue> ValuesOrderedForProcessing(IAttribute attribElm, SaveOptions saveOptions)
        {
            var valuesWithPrimaryFirst = attribElm.Values
                .OrderBy(v =>
                {
                    if(v.Languages == null || !v.Languages.Any()) return 2; // possible primary as no language specified, but not certainly
                    if(v.Languages.Any(l => l.Key == saveOptions.PrimaryLanguage)) return 1; // really primary and marked as such, process this first
                    return 3; // other, work on these last
                });

            // now sort the language definitions to ensure correct handling
            foreach (var value in valuesWithPrimaryFirst)
                value.Languages = value.Languages
                    .OrderBy(l => (l.Key == saveOptions.PrimaryLanguage ? 0 : 1) // first sort-order: primary language yes/no
                        + (l.ReadOnly ? 20 : 10)) // then - place read-only at the end of the list
                    .ToList();
            return valuesWithPrimaryFirst;
        }

        /// <summary>
        /// Merge two attributes, preserving languages as necessary
        /// </summary>
        /// <param name="original"></param>
        /// <param name="update"></param>
        /// <param name="saveOptions"></param>
        /// <returns></returns>
        private static IAttribute MergeAttribute(IAttribute original, IAttribute update, SaveOptions saveOptions)
        {
            // everything in the update will be kept, and optionally some stuff in the original may be preserved
            var result = update;
            foreach (var orgVal in ValuesOrderedForProcessing(original, saveOptions))
            {
                var remainingLanguages = new List<ILanguage>();
                foreach (var valLang in orgVal.Languages) // first process master-languages, then read-only
                {
                    // se if this language has already been set, in that case just leave it
                    var valInResults = result.Values.FirstOrDefault(rv => rv.Languages.Any(rvl => rvl.Key == valLang.Key));
                    if (valInResults == null)
                        // special case: if the original set had named languages, and the new set has no language set (undefined = primary)
                        if (valLang.Key != saveOptions.PrimaryLanguage &&   // make sure it's not the primary language...
                            result.Values.Any(v => v.Languages?.Count == 0)) // and there already is a result with the primary language, just as "undefined"
                            remainingLanguages.Add(valLang);
                }

                // nothing found to keep...
                if (remainingLanguages.Count == 0) continue;

                // Add the value with the remaining languages / relationships
                var val = orgVal.Copy(original.Type);
                val.Languages = remainingLanguages.Select(l => ((Dimension)l).Copy() as ILanguage).ToList();
                result.Values.Add(val);
            }
            return result;
        }

        private static Dictionary<string, IAttribute> KeepOnlyKnownKeys(Dictionary<string, IAttribute> orig, List<string> keys)
        {
            var lowerKeys = keys.Select(k => k.ToLowerInvariant()).ToList();
            return orig.Where(a => lowerKeys.Contains(a.Key.ToLowerInvariant()))
                .ToDictionary(a => a.Key, a => a.Value);
        }

        private static void ImportKnownProperties(Entity newE)
        {
            // check isPublished
            var isPublished = newE.GetBestValue(Constants.EntityFieldIsPublished);
            if (isPublished != null)
            {
                newE.Attributes.Remove(Constants.EntityFieldIsPublished);

                if(isPublished is bool)
                    newE.IsPublished = (bool) isPublished;
                else if (isPublished is string && bool.TryParse(isPublished as string, out bool boolPublished))
                    newE.IsPublished = boolPublished;
            }

            // check isPublished
            var probablyGuid = newE.GetBestValue(Constants.EntityFieldGuid);

            if (probablyGuid != null)
            {
                newE.Attributes.Remove(Constants.EntityFieldGuid);

                if (Guid.TryParse(probablyGuid.ToString(), out Guid eGuid))
                    newE.SetGuid(eGuid);
            }

        }

    }
    
}
