using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Persistence
{
    public class EntitySaver :HasLog
    {
        public EntitySaver(ILog parentLog = null) : base("Dta.Saver", parentLog) { }

        /// <summary>
        /// Goal: Pass changes into an existing entity so that it can then be saved as a whole, with correct
        /// modifications. 
        /// </summary>
        /// <param name="original"></param>
        /// <param name="update"></param>
        /// <param name="saveOptions"></param>
        /// <returns></returns>
        public IEntity CreateMergedForSaving(IEntity original, IEntity update, SaveOptions saveOptions)
        {
            Log.Add($"merge upgrade entity#{original?.EntityId} update#{update?.EntityId} with options:{saveOptions != null}" );
            if (saveOptions == null) throw new ArgumentNullException(nameof(saveOptions));
            Log.Add(() => "opts " + saveOptions?.LogInfo);
            #region Step 0: initial error checks
            if(update == null) // 2017-10-06 2rm removed condition  || update.Attributes?.Count == 0
                throw new Exception("can't prepare entities for saving, no new item with attributes provided");

            var ct = (original ?? update).Type;
            if(ct==null)
                throw new Exception("unknown content-type");

            #endregion

            #region Step 1: check if there is an original item
            // only accept original if it's a real object with a valid GUID, otherwise it's not an existing entity
            var hasOriginal = original != null;
            var originalWasSaved = hasOriginal && !(original.EntityId == 0 && original.EntityGuid == Guid.Empty);
            var idProvidingEntity = originalWasSaved ? original : update;
            #endregion

            #region Step 2: clean up unwanted attributes from both lists

            var origAttribs = original?.Attributes.Copy();
            var newAttribs = update.Attributes.Copy();

            Log.Add($"has orig:{originalWasSaved}, origAtts⋮{origAttribs?.Count}, newAtts⋮{newAttribs.Count}");

            // Optionally remove original values not in the update - but only if no option prevents this
            if (originalWasSaved && !saveOptions.PreserveUntouchedAttributes && !saveOptions.SkipExistingAttributes)
                origAttribs = KeepOnlyKnownKeys(origAttribs, newAttribs.Keys.ToList());

            // Optionaly remove unknown - if possible - of both original and new
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!ct.IsDynamic && !saveOptions.PreserveUnknownAttributes && ct.Attributes != null)
            {
                var keys = ct.Attributes.Select(a => a.Name).ToList();
                keys.Add(Constants.EntityFieldGuid);
                keys.Add(Constants.EntityFieldIsPublished);

                if (originalWasSaved) origAttribs = KeepOnlyKnownKeys(origAttribs, keys);
                newAttribs = KeepOnlyKnownKeys(newAttribs, keys);
            }

            // optionally remove new things which already exist
            if (originalWasSaved && saveOptions.SkipExistingAttributes)
                newAttribs = KeepOnlyKnownKeys(newAttribs, newAttribs.Keys
                    .Where(k => !origAttribs.Keys.Any(
                                ok => string.Equals(k, ok, StringComparison.InvariantCultureIgnoreCase))).ToList());

            #endregion

            #region Step 3: clear unwanted languages as needed

            var hasLanguages = update.GetUsedLanguages().Count + original?.GetUsedLanguages().Count > 0;

            // pre check if languages are properly available for clean-up or merge
            if (hasLanguages && !saveOptions.PreserveUnknownLanguages)
                if ((!saveOptions.Languages?.Any() ?? true)
                    || string.IsNullOrWhiteSpace(saveOptions.PrimaryLanguage)
                    || saveOptions.Languages.All(l => !l.Matches(saveOptions.PrimaryLanguage)))
                    throw new Exception("primary language must exist in languages, cannot continue preparation to save with unclear language setup");


            if (hasLanguages && !saveOptions.PreserveUnknownLanguages && (saveOptions.Languages?.Any() ?? false))
            {
                if (originalWasSaved) StripUnknownLanguages(origAttribs, saveOptions);
                StripUnknownLanguages(newAttribs, saveOptions);
            }

            #endregion

            // now merge into new target
            var mergedAttribs = origAttribs ?? newAttribs; // 2018-03-09 2dm fixed, was previously:  hasOriginal ? origAttribs : newAttribs; // will become 
            if(original != null)
                foreach (var newAttrib in newAttribs)
                    mergedAttribs[newAttrib.Key] = saveOptions.PreserveExistingLanguages && mergedAttribs.ContainsKey(newAttrib.Key)
                        ? MergeAttribute(mergedAttribs[newAttrib.Key], newAttrib.Value, saveOptions)
                        : newAttrib.Value;

            var result = EntityBuilder.FullClone(idProvidingEntity, mergedAttribs, null);
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
        private void StripUnknownLanguages(Dictionary<string, IAttribute> attribs, SaveOptions saveOptions)
        {
            Log.Add("strip unknown langs");
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
                    var newLangs = value.Languages?.Where(l => languages.Any(sysLang => sysLang.Matches(l.Key))).ToList();
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
                    return v.Languages.Any(l => l.Key == saveOptions.PrimaryLanguage) ? 1 : 3; // really primary and marked as such, process this first
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
        private IAttribute MergeAttribute(IAttribute original, IAttribute update, SaveOptions saveOptions)
        {
            Log.Add("merge attribs");
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
                        // to detect this, we must check if we're on primary, and there may be a "undefined" language assignment
                        if (!(valLang.Key == saveOptions.PrimaryLanguage && result.Values.Any(v => v.Languages?.Count == 0))) 
                            remainingLanguages.Add(valLang);
                }

                // nothing found to keep...
                if (remainingLanguages.Count == 0) continue;

                // Add the value with the remaining languages / relationships
                var val = orgVal.Copy(original.Type);
                val.Languages = remainingLanguages.Select(l => ((Language)l).Copy() as ILanguage).ToList();
                result.Values.Add(val);
            }
            return result;
        }

        private Dictionary<string, IAttribute> KeepOnlyKnownKeys(Dictionary<string, IAttribute> orig, List<string> keys)
        {
            Log.Add("keep only known keys");
            var lowerKeys = keys.Select(k => k.ToLowerInvariant()).ToList();
            return orig.Where(a => lowerKeys.Contains(a.Key.ToLowerInvariant()))
                .ToDictionary(a => a.Key, a => a.Value);
        }

        private void ImportKnownProperties(Entity newE)
        {
            Log.Add("import know props");
            // check isPublished
            var isPublished = newE.GetBestValue(Constants.EntityFieldIsPublished);
            if (isPublished != null)
            {
                newE.Attributes.Remove(Constants.EntityFieldIsPublished);

                if(isPublished is bool b)
                    newE.IsPublished = b;
                else if (isPublished is string && bool.TryParse(isPublished as string, out var boolPublished))
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
