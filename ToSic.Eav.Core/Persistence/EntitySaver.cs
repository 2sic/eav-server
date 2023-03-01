using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Persistence
{
    public class EntitySaver : ServiceBase
    {
        public EntitySaver(
            MultiBuilder multiBuilder
        ) : base("Dta.Saver")
        {
            ConnectServices(
                _multiBuilder = multiBuilder
            );
        }

        private readonly MultiBuilder _multiBuilder;


        /// <summary>
        /// Goal: Pass changes into an existing entity so that it can then be saved as a whole, with correct
        /// modifications. 
        /// </summary>
        /// <returns></returns>
        public IEntity CreateMergedForSaving(
            IEntity original,
            IEntity update,
            SaveOptions saveOptions,
            string noParamOrder = Eav.Parameters.Protector,
            int? newId = default,
            //Guid? newGuid = default,
            IContentType newType = default,
            bool logDetails = true
        ) => Log.Func($"entity#{original?.EntityId} update#{update?.EntityId} options:{saveOptions != null}", enabled: logDetails, func: l =>
        {
            if (saveOptions == null) throw new ArgumentNullException(nameof(saveOptions));
            l.A(l.Try(() => "opts " + saveOptions.LogInfo));

            #region Step 0: initial error checks / content-type

            if (update == null)
                throw new Exception("can't prepare entities for saving, no new item with attributes provided");

            var ct = (original ?? update).Type;
            if (ct == null) throw new Exception("unknown content-type");

            #endregion

            #region Step 1: check if there is an original item

            // only accept original if it's a real object with a valid GUID, otherwise it's not an existing entity
            var hasOriginal = original != null;
            var originalWasSaved = hasOriginal && !(original.EntityId == 0 && original.EntityGuid == Guid.Empty);
            var idProvidingEntity = originalWasSaved ? original : update;

            #endregion

            #region Step 2: clean up unwanted attributes from both lists

            var origAttribs = _multiBuilder.Attribute.ListDeepClone(original?.Attributes);
            var newAttribs = _multiBuilder.Attribute.ListDeepClone(update.Attributes);

            l.A($"has orig:{originalWasSaved}, origAtts⋮{origAttribs?.Count}, newAtts⋮{newAttribs.Count}");

            // Optionally remove original values not in the update - but only if no option prevents this
            if (originalWasSaved && !saveOptions.PreserveUntouchedAttributes && !saveOptions.SkipExistingAttributes)
                origAttribs = KeepOnlyKnownKeys(origAttribs, newAttribs.Keys.ToList());

            // Optionaly remove unknown - if possible - of both original and new
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!ct.IsDynamic && !saveOptions.PreserveUnknownAttributes && ct.Attributes != null)
            {
                var keys = ct.Attributes.Select(a => a.Name).ToList();

                keys.Add(Attributes.EntityFieldGuid);
                keys.Add(Attributes.EntityFieldIsPublished);

                // tmp store original IsPublished attribute, will be removed in CorrectPublishedAndGuidImports
                AddIsPublishedAttribute(origAttribs, original?.IsPublished);
                // tmp store update IsPublished attribute, will be removed in CorrectPublishedAndGuidImports
                AddIsPublishedAttribute(newAttribs, update.IsPublished);

                if (originalWasSaved) origAttribs = KeepOnlyKnownKeys(origAttribs, keys);
                newAttribs = KeepOnlyKnownKeys(newAttribs, keys);
            }

            // optionally remove new things which already exist
            if (originalWasSaved && saveOptions.SkipExistingAttributes)
                newAttribs = KeepOnlyKnownKeys(newAttribs, newAttribs.Keys
                    .Where(k => !origAttribs.Keys.Any(k.EqualsInsensitive)).ToList());

            #endregion

            #region Step 3: clear unwanted languages as needed

            var hasLanguages = update.GetUsedLanguages().Count + original?.GetUsedLanguages().Count > 0;

            // pre check if languages are properly available for clean-up or merge
            if (hasLanguages && !saveOptions.PreserveUnknownLanguages)
                if ((!saveOptions.Languages?.Any() ?? true)
                    || string.IsNullOrWhiteSpace(saveOptions.PrimaryLanguage)
                    || saveOptions.Languages.All(lang => !lang.Matches(saveOptions.PrimaryLanguage)))
                    throw new Exception(
                        "primary language must exist in languages, cannot continue preparation to save with unclear language setup");


            if (hasLanguages && !saveOptions.PreserveUnknownLanguages && (saveOptions.Languages?.Any() ?? false))
            {
                if (originalWasSaved) StripUnknownLanguages(origAttribs, saveOptions);
                StripUnknownLanguages(newAttribs, saveOptions);
            }

            #endregion

            // now merge into new target
            var mergedAttribs = origAttribs ?? newAttribs;
            if (original != null)
                foreach (var newAttrib in newAttribs)
                    mergedAttribs[newAttrib.Key] = saveOptions.PreserveExistingLanguages &&
                                                   mergedAttribs.ContainsKey(newAttrib.Key)
                        ? MergeAttribute(mergedAttribs[newAttrib.Key], newAttrib.Value, saveOptions)
                        : newAttrib.Value;

            var preCleaned = CorrectPublishedAndGuidImports(mergedAttribs, logDetails);
            var clone = _multiBuilder.Entity.Clone(idProvidingEntity, id: newId, guid: preCleaned.NewGuid, type: newType, values: preCleaned.Attributes, isPublished: preCleaned.NewIsPublished);
            //var result = CorrectPublishedAndGuidImports(clone, clone.Attributes, logDetails); // as Entity;
            return (clone, "ok");
        });

        private void AddIsPublishedAttribute(IDictionary<string, IAttribute> attributes, bool? isPublished) 
        {
            if (isPublished.HasValue && !attributes.ContainsKey(Attributes.EntityFieldIsPublished)) 
                attributes.Add(Attributes.EntityFieldIsPublished, CreateIsPublishedAttribute(isPublished.Value));
        }


        private IAttribute CreateIsPublishedAttribute(bool isPublished)
        {
            var values = new List<IValue> { _multiBuilder.Value.Build(ValueTypes.Boolean.ToString(), isPublished) };
            var attribute = _multiBuilder.Attribute.CreateTyped(Attributes.EntityFieldIsPublished, ValueTypes.Boolean, values);
            // #immutable
            //attribute.Values = values;
            return attribute;
        }


        /// <summary>
        /// Will remove all language-information for values which have no language
        /// </summary>
        /// <param name="allFields"></param>
        /// <param name="saveOptions"></param>
        /// <remarks>
        /// this expects that saveOptions contain Languages & PrimaryLanguage, and that this is reliable
        /// </remarks>
        private void StripUnknownLanguages(Dictionary<string, IAttribute> allFields, SaveOptions saveOptions) => Log.Do(() =>
        {
            var languages = saveOptions.Languages;

            foreach (var field in allFields)
            {
                var values = new List<IValue>();       // new empty values list

                // when we go through the values, we should always take the primary language first
                // this is detectable by having either no language, or having the primary language
                var orderedValues = ValuesOrderedForProcessing(field.Value.Values, saveOptions);
                foreach (var value in orderedValues)
                {
                    // create filtered list of languages
                    var newLangs = value.Languages?
                        .Where(l => languages.Any(sysLang => sysLang.Matches(l.Key)))
                        .ToImmutableList();
                    // only keep this value, if it is either the first (so contains primary or null-language) or that it still has a remaining language assignment
                    if (values.Any() && !(newLangs?.Any() ?? false)) continue;
                    //value.Languages = newLangs;
                    values.Add(value.Clone(newLangs));
                }
                field.Value.Values = values;
            }
        });

        private static IList<IValue> ValuesOrderedForProcessing(IList<IValue> values, SaveOptions saveOptions)
        {
            var valuesWithPrimaryFirst = values
                .OrderBy(v =>
                {
                    if(v.Languages == null || !v.Languages.Any()) return 2; // possible primary as no language specified, but not certainly
                    return v.Languages.Any(l => l.Key == saveOptions.PrimaryLanguage) ? 1 : 3; // really primary and marked as such, process this first
                });

            // now sort the language definitions to ensure correct handling
            var valsWithLanguagesSorted = valuesWithPrimaryFirst
                .Select(value =>
                {
                    var sortedLangs = value.Languages.OrderBy(l =>
                            (l.Key == saveOptions.PrimaryLanguage ? 0 : 1) // first sort-order: primary language yes/no
                            + (l.ReadOnly ? 20 : 10)) // then - place read-only at the end of the list
                        .ToImmutableList();
                    return value.Clone(sortedLangs);
                })
                .ToList();
            return valsWithLanguagesSorted;
        }

        /// <summary>
        /// Merge two attributes, preserving languages as necessary
        /// </summary>
        /// <param name="original"></param>
        /// <param name="update"></param>
        /// <param name="saveOptions"></param>
        /// <returns></returns>
        private IAttribute MergeAttribute(IAttribute original, IAttribute update, SaveOptions saveOptions) => Log.Func(() =>
        {
            // everything in the update will be kept, and optionally some stuff in the original may be preserved
            var result = update;
            foreach (var orgVal in ValuesOrderedForProcessing(original.Values, saveOptions))
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
                // 2023-02-24 2dm optimized this, keep comment till ca. 2023-04 in case something breaks
                //var languagesToUse = remainingLanguages.Select(l => LanguageBuilder.Clone(l) as ILanguage).ToList();
                var languagesToUse = LanguageBuilder.Clone(remainingLanguages);
                var val = _multiBuilder.Value.Clone(orgVal, original.Type, languagesToUse);
                result.Values.Add(val);
            }

            return result;
        });

        private DimensionBuilder LanguageBuilder => _langBuilder ?? (_langBuilder = new DimensionBuilder());
        private DimensionBuilder _langBuilder;

        private Dictionary<string, IAttribute> KeepOnlyKnownKeys(Dictionary<string, IAttribute> orig, List<string> keys)
        {
            var callLog = Log.Fn<Dictionary<string, IAttribute>>("keep only known keys");
            var lowerKeys = keys.Select(k => k.ToLowerInvariant()).ToList();
            var result = orig
                .Where(a => lowerKeys.Contains(a.Key.ToLowerInvariant()))
                .ToDictionary(a => a.Key, a => a.Value);
            return callLog.Return(result, $"{result.Count}");
        }

        private (/*IEntity Entity,*/ Dictionary<string, IAttribute> Attributes, Guid? NewGuid, bool? NewIsPublished)
            CorrectPublishedAndGuidImports(/*IEntity newE,*/ Dictionary<string, IAttribute> values, bool logDetails
            ) => Log.Func(enabled: logDetails, func: l =>
        {
            // check IsPublished
            //var isPublished = newE.Value(Attributes.EntityFieldIsPublished);
            values.TryGetValue(Attributes.EntityFieldIsPublished, out var isPublishedAttr);
            var isPublished = isPublishedAttr?.GetTypedValue(Array.Empty<string>()).Result;
            bool? newIsPublished = null;
            if (isPublished != null)
            {
                l.A("Found property for published, will move");
                values.Remove(Attributes.EntityFieldIsPublished);

                //var temp = isPublished.GetTypedValue(Array.Empty<string>());
                if (isPublished is bool b)
                    newIsPublished = b;
                //newE.IsPublished = b;
                else if (isPublished is string sPublished && bool.TryParse(sPublished, out var boolPublished))
                    newIsPublished = boolPublished;
                    //newE.IsPublished = boolPublished;
            }

            // check EntityGuid
            //var probablyGuid = newE.Value(Attributes.EntityFieldGuid);
            values.TryGetValue(Attributes.EntityFieldGuid, out var probablyGuidAttr);
            var probablyGuid = probablyGuidAttr?.GetTypedValue(Array.Empty<string>()).Result;
            Guid? newGuid = null;
            if (probablyGuid != null)
            {
                l.A("Found property for published, will move");
                values.Remove(Attributes.EntityFieldGuid);
                if (Guid.TryParse(probablyGuid.ToString(), out var eGuid))
                    newGuid = eGuid;
                    //newE.SetGuid(eGuid);
            }

            //var cloned = _multiBuilder.Entity.ResetIdentifiers(newE, newGuid: newGuid, isPublished: newIsPublished);

            return ((/*newEnt,*/ values, newGuid, newIsPublished), "ok");
        });

    }
    
}
