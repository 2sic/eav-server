﻿using ToSic.Eav.Data.Build;
using ToSic.Lib.Coding;
using static System.StringComparer;

namespace ToSic.Eav.Data.Sys.Save;

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
        IContentType? newType = default,
        bool logDetails = true
    )
    {
        var l = (logDetails ? Log : null).Fn<IEntity>($"entity#{original?.EntityId} update#{update?.EntityId} options:{saveOptions != null}", timer: true);
        
        if (saveOptions == null)
            throw new ArgumentNullException(nameof(saveOptions));

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
        var originalWasSaved = hasOriginal && !(original!.EntityId == 0 && original.EntityGuid == Guid.Empty);
        var idProvidingEntity = originalWasSaved && original != null // re-check original != null so compiler knows that idProvidingEntity is not null
            ? original
            : update;

        #endregion

        #region Step 2: clean up unwanted attributes from both lists

        var origAttribsOrNull = original != null
            ? dataBuilder.Attribute.Mutable(original.Attributes)
            : null;
        var newAttribs = dataBuilder.Attribute.Mutable(update.Attributes);

        l.A($"has orig:{originalWasSaved}, origAtts⋮{origAttribsOrNull?.Count}, newAtts⋮{newAttribs.Count}");

        // Optionally remove original values not in the update - but only if no option prevents this
        if (originalWasSaved && saveOptions is { PreserveUntouchedAttributes: false, SkipExistingAttributes: false })
            origAttribsOrNull = KeepOnlyKnownKeys(origAttribsOrNull! /* call only happens when original != null */, newAttribs.Keys.ToListOpt());

        // Optionally remove unknown - if possible - of both original and new
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (!ct.IsDynamic && !saveOptions.PreserveUnknownAttributes && ct.Attributes != null)
        {
            var keys = ct.Attributes
                .Select(a => a.Name)
                .ToList();

            keys.Add(AttributeNames.EntityFieldGuid);
            keys.Add(AttributeNames.EntityFieldIsPublished);

            // TODO: NOTE this looks wrong - as it would null-error if origAttributes were null
            // tmp store original IsPublished attribute, will be removed in CorrectPublishedAndGuidImports
            AddIsPublishedAttribute(origAttribsOrNull, original?.IsPublished);
            // tmp store update IsPublished attribute, will be removed in CorrectPublishedAndGuidImports
            AddIsPublishedAttribute(newAttribs, update.IsPublished);

            if (originalWasSaved)
                origAttribsOrNull = KeepOnlyKnownKeys(origAttribsOrNull, keys);
            newAttribs = KeepOnlyKnownKeys(newAttribs, keys);
        }

        // optionally remove new things which already exist
        if (originalWasSaved && saveOptions.SkipExistingAttributes && origAttribsOrNull != null)
            newAttribs = KeepOnlyKnownKeys(newAttribs,
                newAttribs.Keys
                .Where(k => !origAttribsOrNull.Keys.Any(k.EqualsInsensitive))
                .ToListOpt()
            );

        #endregion

        #region Step 3: clear unwanted languages as needed

        var hasLanguages = update.GetUsedLanguages().Count + original?.GetUsedLanguages().Count > 0;

        // pre-check if languages are properly available for clean-up or merge
        if (hasLanguages && !saveOptions.PreserveUnknownLanguages)
            if ((!saveOptions.Languages?.Any() ?? true)
                || string.IsNullOrWhiteSpace(saveOptions.PrimaryLanguage)
                || saveOptions.Languages.All(lang => !lang.Matches(saveOptions.PrimaryLanguage)))
                throw new(
                    "primary language must exist in languages, cannot continue preparation to save with unclear language setup");


        if (hasLanguages && !saveOptions.PreserveUnknownLanguages && saveOptions.Languages.SafeAny())
        {
            if (originalWasSaved)
                origAttribsOrNull = StripUnknownLanguages(origAttribsOrNull!, saveOptions);
            newAttribs = StripUnknownLanguages(newAttribs, saveOptions);
        }

        #endregion

        // now merge into new target
        var mergedAttribs = origAttribsOrNull ?? newAttribs;
        if (original != null)
            foreach (var newAttrib in newAttribs)
            {
                mergedAttribs[newAttrib.Key] = saveOptions.PreserveExistingLanguages &&
                                            mergedAttribs.TryGetValue(newAttrib.Key, out var oldAttribute)
                    ? MergeAttribute(oldAttribute, newAttrib.Value, saveOptions)
                    : newAttrib.Value;
            }

        var preCleaned = CorrectPublishedAndGuidImports(mergedAttribs, logDetails);
        var clone = dataBuilder.Entity.CreateFrom(
            idProvidingEntity,
            id: newId,
            guid: preCleaned.NewGuid,
            type: newType,
            attributes: dataBuilder.Attribute.Create(preCleaned.Attributes),
            isPublished: preCleaned.NewIsPublished);
        return l.ReturnAsOk(clone);
    }

    private void AddIsPublishedAttribute(IDictionary<string, IAttribute> attributes, bool? isPublished) 
    {
        if (isPublished.HasValue && !attributes.ContainsKey(AttributeNames.EntityFieldIsPublished)) 
            attributes.Add(AttributeNames.EntityFieldIsPublished, CreateIsPublishedAttribute(isPublished.Value));
    }


    private IAttribute CreateIsPublishedAttribute(bool isPublished)
    {
        var values = new List<IValue> { dataBuilder.Value.Bool(isPublished, []) };
        var attribute = dataBuilder.Attribute.Create(AttributeNames.EntityFieldIsPublished, ValueTypes.Boolean, values);
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

        var modified = allFields
            .ToDictionary(
                pair => pair.Key,
                pair =>
                {
                    var values = new List<IValue>(); // new empty values list

                    // when we go through the values, we should always take the primary language first
                    // this is detectable by having either no language, or having the primary language
                    var orderedValues = ValuesOrderedForProcessing(pair.Value.Values, saveOptions);
                    foreach (var value in orderedValues)
                    {
                        // create filtered list of languages
                        var newLangs = value.Languages?
                            .Where(lng => languages.Any(sysLang => sysLang.Matches(lng.Key)))
                            .ToImmutableSafe();
                        // only keep this value, if it is either the first (so contains primary or null-language)
                        // ...or that it still has a remaining language assignment
                        if (values.Any() && !(newLangs?.Any() ?? false))
                            continue;
                        values.Add(value.With(newLangs!));
                    }

                    //field.Value.Values = values;
                    return dataBuilder.Attribute.CreateFrom(pair.Value, values.ToImmutableSafe());
                },
                InvariantCultureIgnoreCase
            );

        return l.Return(modified);
    }

    private static IList<IValue> ValuesOrderedForProcessing(IEnumerable<IValue> values, SaveOptions saveOptions)
    {
        var valuesWithPrimaryFirst = values
            .OrderBy(v =>
            {
                if(v.Languages == null || !v.Languages.Any())
                    return 2; // possible primary as no language specified, but not certainly
                return v.Languages.Any(l => l.Key == saveOptions.PrimaryLanguage) ? 1 : 3; // really primary and marked as such, process this first
            });

        // now sort the language definitions to ensure correct handling
        var valsWithLanguagesSorted = valuesWithPrimaryFirst
            .Select(value =>
            {
                var sortedLangs = value.Languages.OrderBy(l =>
                        (l.Key == saveOptions.PrimaryLanguage ? 0 : 1) // first sort-order: primary language yes/no
                        + (l.ReadOnly ? 20 : 10)) // then - place read-only at the end of the list
                    .ToImmutableSafe();
                return value.With(sortedLangs);
            })
            .ToListOpt();
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
            if (remainingLanguages.Count == 0)
                continue;

            // Add the value with the remaining languages / relationships
            var val = dataBuilder.Value.CreateFrom(orgVal, languages: remainingLanguages.ToImmutableSafe());
            result.Add(val);
        }

        var final = dataBuilder.Attribute.CreateFrom(update, result.ToImmutableSafe());
        return l.Return(final);
    }
    
    private IDictionary<string, IAttribute> KeepOnlyKnownKeys(IDictionary<string, IAttribute> orig, IEnumerable<string> keys)
    {
        var l = Log.Fn<IDictionary<string, IAttribute>>();
        var lowerKeys = keys
            .Select(k => k.ToLowerInvariant())
            .ToListOpt();
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
        values.TryGetValue(AttributeNames.EntityFieldIsPublished, out var isPublishedAttr);
        var isPublished = isPublishedAttr?.Values.FirstOrDefault()?.ObjectContents;
        bool? newIsPublished = null;
        if (isPublished != null)
        {
            l.A("Found property for published, will move");
            values.Remove(AttributeNames.EntityFieldIsPublished);

            if (isPublished is bool b)
                newIsPublished = b;
            else if (isPublished is string sPublished && bool.TryParse(sPublished, out var boolPublished))
                newIsPublished = boolPublished;
        }

        // check EntityGuid
        values.TryGetValue(AttributeNames.EntityFieldGuid, out var probablyGuidAttr);
        var probablyGuid = probablyGuidAttr?.Values.FirstOrDefault()?.ObjectContents;
        Guid? newGuid = null;
        if (probablyGuid != null)
        {
            l.A("Found property for published, will move");
            values.Remove(AttributeNames.EntityFieldGuid);
            if (Guid.TryParse(probablyGuid.ToString(), out var eGuid))
                newGuid = eGuid;
        }

        return l.Return((values, newGuid, newIsPublished), "ok");
    }

}