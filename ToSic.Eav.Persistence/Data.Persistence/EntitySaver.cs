using System.Collections.Immutable;
using ToSic.Eav.Data.Build;
using ToSic.Lib.Coding;
using static System.StringComparer;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Persistence;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EntitySaver(DataBuilder dataBuilder) : ServiceBase("Dta.Saver", connect: [dataBuilder])
{
    /// <summary>
    /// Goal: Pass changes into an existing entity so that it can then be saved as a whole, with correct
    /// modifications. 
    /// </summary>
    /// <returns></returns>
    public IEntity CreateMergedForSaving(
        IEntity original,
        IEntity update,
        SaveOptions saveOptions,
        NoParamOrder noParamOrder = default,
        int? newId = default,
        IContentType newType = default,
        bool logDetails = true
    )
    {
        var l = (logDetails ? Log : null).Fn<IEntity>($"entity#{original?.EntityId} update#{update?.EntityId} options:{saveOptions != null}", timer: true);
        if (saveOptions == null) throw new ArgumentNullException(nameof(saveOptions));
        l.A(l.Try(() => "opts " + saveOptions));

        #region Step 0: initial error checks / content-type

        if (update == null)
            throw new("can't prepare entities for saving, no new item with attributes provided");

        var ct = (original ?? update).Type;
        if (ct == null) throw new("unknown content-type");

        #endregion

        #region Step 1: check if there is an original item

        // only accept original if it's a real object with a valid GUID, otherwise it's not an existing entity
        var hasOriginal = original != null;
        var originalWasSaved = hasOriginal && !(original.EntityId == 0 && original.EntityGuid == Guid.Empty);
        var idProvidingEntity = originalWasSaved ? original : update;

        #endregion

        #region Step 2: clean up unwanted attributes from both lists

        var origAttribsOrNull = original == null ? null : dataBuilder.Attribute.Mutable(original.Attributes);
        var newAttribs = dataBuilder.Attribute.Mutable(update.Attributes);

        l.A($"has orig:{originalWasSaved}, origAtts⋮{origAttribsOrNull?.Count}, newAtts⋮{newAttribs.Count}");

        // Optionally remove original values not in the update - but only if no option prevents this
        if (originalWasSaved && !saveOptions.PreserveUntouchedAttributes && !saveOptions.SkipExistingAttributes)
            origAttribsOrNull = KeepOnlyKnownKeys(origAttribsOrNull, newAttribs.Keys.ToList());

        // Optionaly remove unknown - if possible - of both original and new
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (!ct.IsDynamic && !saveOptions.PreserveUnknownAttributes && ct.Attributes != null)
        {
            var keys = ct.Attributes.Select(a => a.Name).ToList();

            keys.Add(Attributes.EntityFieldGuid);
            keys.Add(Attributes.EntityFieldIsPublished);

            // TODO: NOTE this looks wrong - as it would null-error if origAttributes were null
            // tmp store original IsPublished attribute, will be removed in CorrectPublishedAndGuidImports
            AddIsPublishedAttribute(origAttribsOrNull, original?.IsPublished);
            // tmp store update IsPublished attribute, will be removed in CorrectPublishedAndGuidImports
            AddIsPublishedAttribute(newAttribs, update.IsPublished);

            if (originalWasSaved) origAttribsOrNull = KeepOnlyKnownKeys(origAttribsOrNull, keys);
            newAttribs = KeepOnlyKnownKeys(newAttribs, keys);
        }

        // optionally remove new things which already exist
        if (originalWasSaved && saveOptions.SkipExistingAttributes)
            newAttribs = KeepOnlyKnownKeys(newAttribs, newAttribs.Keys
                .Where(k => !origAttribsOrNull.Keys.Any(k.EqualsInsensitive)).ToList());

        #endregion

        #region Step 3: clear unwanted languages as needed

        var hasLanguages = update.GetUsedLanguages().Count + original?.GetUsedLanguages().Count > 0;

        // pre check if languages are properly available for clean-up or merge
        if (hasLanguages && !saveOptions.PreserveUnknownLanguages)
            if ((!saveOptions.Languages?.Any() ?? true)
                || string.IsNullOrWhiteSpace(saveOptions.PrimaryLanguage)
                || saveOptions.Languages.All(lang => !lang.Matches(saveOptions.PrimaryLanguage)))
                throw new(
                    "primary language must exist in languages, cannot continue preparation to save with unclear language setup");


        if (hasLanguages && !saveOptions.PreserveUnknownLanguages && saveOptions.Languages.SafeAny())
        {
            if (originalWasSaved) origAttribsOrNull = StripUnknownLanguages(origAttribsOrNull, saveOptions);
            newAttribs = StripUnknownLanguages(newAttribs, saveOptions);
        }

        #endregion

        // now merge into new target
        var mergedAttribs = origAttribsOrNull ?? newAttribs;
        if (original != null)
            foreach (var newAttrib in newAttribs)
                mergedAttribs[newAttrib.Key] = saveOptions.PreserveExistingLanguages &&
                                               mergedAttribs.TryGetValue(newAttrib.Key, out var oldAttribute)
                    ? MergeAttribute(oldAttribute, newAttrib.Value, saveOptions)
                    : newAttrib.Value;

        var preCleaned = CorrectPublishedAndGuidImports(mergedAttribs, logDetails);
        var clone = dataBuilder.Entity.CreateFrom(idProvidingEntity,
            id: newId,
            guid: preCleaned.NewGuid,
            type: newType,
            attributes: dataBuilder.Attribute.Create(preCleaned.Attributes),
            isPublished: preCleaned.NewIsPublished);
        return l.ReturnAsOk(clone);
    }

    private void AddIsPublishedAttribute(IDictionary<string, IAttribute> attributes, bool? isPublished) 
    {
        if (isPublished.HasValue && !attributes.ContainsKey(Attributes.EntityFieldIsPublished)) 
            attributes.Add(Attributes.EntityFieldIsPublished, CreateIsPublishedAttribute(isPublished.Value));
    }


    private IAttribute CreateIsPublishedAttribute(bool isPublished)
    {
        var values = new List<IValue> { dataBuilder.Value.Bool(isPublished, []) };
        var attribute = dataBuilder.Attribute.Create(Attributes.EntityFieldIsPublished, ValueTypes.Boolean, values);
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
    private IDictionary<string, IAttribute> StripUnknownLanguages(IDictionary<string, IAttribute> allFields, SaveOptions saveOptions)
    {
        var l = Log.Fn<IDictionary<string, IAttribute>>();
        var languages = saveOptions.Languages;

        var modified = allFields.ToDictionary(
            pair => pair.Key,
            field =>
            {
                var values = new List<IValue>(); // new empty values list

                // when we go through the values, we should always take the primary language first
                // this is detectable by having either no language, or having the primary language
                var orderedValues = ValuesOrderedForProcessing(field.Value.Values, saveOptions);
                foreach (var value in orderedValues)
                {
                    // create filtered list of languages
                    var newLangs = value.Languages?
                        .Where(l => languages.Any(sysLang => sysLang.Matches(l.Key)))
                        .ToImmutableList();
                    // only keep this value, if it is either the first (so contains primary or null-language)
                    // ...or that it still has a remaining language assignment
                    if (values.Any() && !(newLangs?.Any() ?? false)) continue;
                    values.Add(value.With(newLangs));
                }

                //field.Value.Values = values;
                return dataBuilder.Attribute.CreateFrom(field.Value, values.ToImmutableList());
            }, InvariantCultureIgnoreCase);

        return l.Return(modified);
    }

    private static IList<IValue> ValuesOrderedForProcessing(IEnumerable<IValue> values, SaveOptions saveOptions)
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
                return value.With(sortedLangs);
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
    private IAttribute MergeAttribute(IAttribute original, IAttribute update, SaveOptions saveOptions)
    {
        var l = Log.Fn<IAttribute>();
        // everything in the update will be kept, and optionally some stuff in the original may be preserved
        var result = update.Values.ToList();
        foreach (var orgVal in ValuesOrderedForProcessing(original.Values, saveOptions))
        {
            var remainingLanguages = new List<ILanguage>();
            foreach (var valLang in orgVal.Languages) // first process master-languages, then read-only
            {
                // se if this language has already been set, in that case just leave it
                var valInResults = result.FirstOrDefault(rv => rv.Languages.Any(rvl => rvl.Key == valLang.Key));
                if (valInResults == null)
                    // special case: if the original set had named languages, and the new set has no language set (undefined = primary)
                    // to detect this, we must check if we're on primary, and there may be a "undefined" language assignment
                    if (!(valLang.Key == saveOptions.PrimaryLanguage && result.Any(v => v.Languages?.Count() == 0)))
                        remainingLanguages.Add(valLang);
            }

            // nothing found to keep...
            if (remainingLanguages.Count == 0) continue;

            // Add the value with the remaining languages / relationships
            var val = dataBuilder.Value.CreateFrom(orgVal, languages: remainingLanguages.ToImmutableList());
            result.Add(val);
        }

        var final = dataBuilder.Attribute.CreateFrom(update, result.ToImmutableList());
        return l.Return(final);
    }

    // 2023-03-23 2dm - this new more functional code didn't produce the same result, so disabling it for now. Tests not passing with this.
    ///// <summary>
    ///// Merge two attributes, preserving languages as necessary
    ///// </summary>
    ///// <param name="original"></param>
    ///// <param name="update"></param>
    ///// <param name="saveOptions"></param>
    ///// <returns></returns>
    //private IAttribute MergeAttributeNewNotWorkingProperly(IAttribute original, IAttribute update, SaveOptions saveOptions) => Log.Func(() =>
    //{
    //    var values = ValuesOrderedForProcessing(original.Values, saveOptions)
    //        .Select(orgVal =>
    //        {
    //            var remainingLanguages = new List<ILanguage>();
    //            foreach (var valLang in orgVal.Languages) // first process master-languages, then read-only
    //            {
    //                // se if this language has already been set, in that case just leave it
    //                var valInResults =
    //                    update.Values.FirstOrDefault(rv => rv.Languages.Any(rvl => rvl.Key == valLang.Key));
    //                if (valInResults == null)
    //                    // special case: if the original set had named languages, and the new set has no language set (undefined = primary)
    //                    // to detect this, we must check if we're on primary, and there may be a "undefined" language assignment
    //                    if (!(valLang.Key == saveOptions.PrimaryLanguage &&
    //                          update.Values.Any(v => v.Languages?.Count() == 0)))
    //                        remainingLanguages.Add(valLang);
    //            }

    //            // nothing found to keep...
    //            if (remainingLanguages.Count == 0) return null;

    //            // Add the value with the remaining languages / relationships
    //            // 2023-02-24 2dm optimized this, keep comment till ca. 2023-04 in case something breaks
    //            //var languagesToUse = remainingLanguages.Select(l => LanguageBuilder.Clone(l) as ILanguage).ToList();
    //            //var languagesToUse = LanguageBuilder.Clone(remainingLanguages);
    //            var val = _dataBuilder.Value.CreateFrom(orgVal, languages: remainingLanguages.ToImmutableList());
    //            return val;
    //        })
    //        .Where(val => val != null)
    //        .ToImmutableList();

    //    // everything in the update will be kept, and optionally some stuff in the original may be preserved
    //    var result = _dataBuilder.Attribute.CreateFrom(update, values);

    //    return result;
    //});

    private IDictionary<string, IAttribute> KeepOnlyKnownKeys(IDictionary<string, IAttribute> orig, List<string> keys)
    {
        var l = Log.Fn<IDictionary<string, IAttribute>>();
        var lowerKeys = keys.Select(k => k.ToLowerInvariant()).ToList();
        var result = orig
            .Where(a => lowerKeys.Contains(a.Key.ToLowerInvariant()))
            .ToDictionary(a => a.Key, a => a.Value);
        return l.Return(result, $"{result.Count}");
    }

    private (IDictionary<string, IAttribute> Attributes, Guid? NewGuid, bool? NewIsPublished)
        CorrectPublishedAndGuidImports(IDictionary<string, IAttribute> values, bool logDetails) 
    {
        var l = (logDetails ? Log : null)
            .Fn<(IDictionary<string, IAttribute> Attributes, Guid? NewGuid, bool? NewIsPublished)>();
        // check IsPublished
        values.TryGetValue(Attributes.EntityFieldIsPublished, out var isPublishedAttr);
        var isPublished = isPublishedAttr?.Values.FirstOrDefault()?.ObjectContents;
        bool? newIsPublished = null;
        if (isPublished != null)
        {
            l.A("Found property for published, will move");
            values.Remove(Attributes.EntityFieldIsPublished);

            if (isPublished is bool b)
                newIsPublished = b;
            else if (isPublished is string sPublished && bool.TryParse(sPublished, out var boolPublished))
                newIsPublished = boolPublished;
        }

        // check EntityGuid
        values.TryGetValue(Attributes.EntityFieldGuid, out var probablyGuidAttr);
        var probablyGuid = probablyGuidAttr?.Values.FirstOrDefault()?.ObjectContents;
        Guid? newGuid = null;
        if (probablyGuid != null)
        {
            l.A("Found property for published, will move");
            values.Remove(Attributes.EntityFieldGuid);
            if (Guid.TryParse(probablyGuid.ToString(), out var eGuid))
                newGuid = eGuid;
        }

        return l.Return((values, newGuid, newIsPublished), "ok");
    }

}