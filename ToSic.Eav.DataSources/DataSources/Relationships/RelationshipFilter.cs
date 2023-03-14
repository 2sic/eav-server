using System;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// Filter Entities by Value in a Related Entity. For example:
    /// Find all Books (desired Entity), whose Authors (related Entity) have a Country (Attribute) with 'Switzerland' (Value). 
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    [VisualQuery(
        NiceName = "Relationship Filter",
        UiHint = "Keep items having a relationship matching a criteria",
        Icon = Icons.Share,
        Type = DataSourceType.Filter,
        GlobalName = "ToSic.Eav.DataSources.RelationshipFilter, ToSic.Eav.DataSources",
        In = new[] { QueryConstants.InStreamDefaultRequired, DataSourceConstants.StreamFallbackName },
        DynamicOut = false,
        ExpectsDataOfType = "|Config ToSic.Eav.DataSources.RelationshipFilter",
        HelpLink = "https://r.2sxc.org/DsRelationshipFilter")]
    public sealed class RelationshipFilter : DataSource
    {
        #region Configuration-properties

        [PrivateApi] internal const string FieldAttributeOnRelationship = "AttributeOnRelationship";
        [PrivateApi] internal const string FieldComparison = "Comparison";
        [PrivateApi] internal const string FieldDirection = "Direction";

        [PrivateApi] private const string PrefixNot = "not-";
        [PrivateApi] internal const string DefaultDirection = "child";
        [PrivateApi] private const string DefaultSeparator = "ignore"; // by default, don't separate!
        [PrivateApi] private readonly string[] _directionPossibleValues = { DefaultDirection };


        /// <summary>
        /// default case - must contain all provided filter-values
        /// </summary>
        [PrivateApi] internal const string CompareModeContains = "contains";
        /// <summary>
        /// must contain any of the provided filter-values
        /// </summary>
        [PrivateApi] internal const string CompareModeContainsAny = "containsany";
        /// <summary>
        /// must contain anything (not empty)
        /// </summary>
        [PrivateApi] internal const string CompareModeAny = "any";
        /// <summary>
        /// first item must match provided filter-values
        /// </summary>
        [PrivateApi] internal const string CompareModeFirst = "first";
        /// <summary>
        /// Count amount of items in the relationship
        /// </summary>
        [PrivateApi] internal const string CompareModeCount = "count";
        /// <summary>
        /// All valid compare modes
        /// </summary>
        [PrivateApi] internal string[] AllCompareModes = { CompareModeContains, CompareModeContainsAny, CompareModeAny, CompareModeFirst, CompareModeCount };


        private enum CompareType { Any, Id, Title, Auto }

        /// <summary>
        /// Relationship-attribute - in the example this would be 'Author' as we're checking values in related Author items. 
        /// </summary>
        [Configuration]
        public string Relationship
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        /// <summary>
        /// The filter-value that will be used - for example "Switzerland" when looking for authors from there
        /// </summary>
        [Configuration]
        public string Filter
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        /// <summary>
        /// The attribute we're looking into, in this case it would be 'Country' because we're checking what Authors are from Switzerland.
        /// </summary>
        [Configuration(Field = FieldAttributeOnRelationship, Fallback = Attributes.EntityFieldTitle)]
		public string CompareAttribute
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        /// <summary>
        /// Comparison mode.
        /// "default" and "contains" will check if such a relationship is available
        /// other modes like "equals" or "exclude" not implemented
        /// </summary>
        [Configuration(Field = FieldComparison, Fallback = CompareModeContains)]
        public string CompareMode
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value.ToLowerInvariant());
        }

        /// <summary>
        /// Separator value where we have multiple values / IDs to compare. Default is 'ignore' = no separator
        /// </summary>
        [Configuration(Fallback = DefaultSeparator)]
		public string Separator
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value.ToLowerInvariant());
        }

        /// <summary>
        /// Determines if the relationship we're looking into is a 'child'-relationship (default) or 'parent' relationship.
        /// </summary>
        [Configuration(Field = FieldDirection, Fallback = DefaultDirection)]
		public string ChildOrParent
        {
            get => Configuration.GetThis();
            set
            {
                var valLower = value.ToLowerInvariant();
                if (!_directionPossibleValues.Contains(valLower))
                    throw new Exception("Value '" + value + "'not allowed for ChildOrParent");
                Configuration.SetThis(valLower);
            }
        }

        #endregion

        /// <summary>
        /// Constructs a new RelationshipFilter
        /// </summary>
        [PrivateApi]
        public RelationshipFilter(MyServices services): base(services, $"{DataSourceConstants.LogPrefix}.Relfil")
        {
            Provide(GetRelationshipsOrFallback);
            //ConfigMask(nameof(Relationship));
            //ConfigMask(nameof(Filter));
            //ConfigMaskMyConfig(nameof(CompareAttribute), $"{Settings.AttributeOnRelationship}||{Attributes.EntityFieldTitle}");
            //ConfigMaskMyConfig(nameof(CompareMode), $"{Settings.Comparison}||{CompareModes.contains}");
            //ConfigMask($"{nameof(Separator)}||{DefaultSeparator}");
            // todo: unclear if implemented...
            //ConfigMaskMyConfig(nameof(ChildOrParent), $"{Settings.Direction}||{DefaultDirection}");
        }

        private IImmutableList<IEntity> GetRelationshipsOrFallback() => Log.Func(() =>
        {
            var res = GetEntities();
            if (!res.Any() && In.HasStreamWithItems(DataSourceConstants.StreamFallbackName))
                return (In[DataSourceConstants.StreamFallbackName].List.ToImmutableList(), "fallback");

            return (res, "ok");
        });

        private IImmutableList<IEntity> GetEntities() => Log.Func(l =>
        {
            // todo: maybe do something about languages on properties?
            Configuration.Parse();

            var relationship = Relationship;
            var compAttr = CompareAttribute;
            var filter = Filter.ToLowerInvariant(); // new: make case insensitive
            var strMode = CompareMode.ToLowerInvariant();
            var useNot = strMode.StartsWith(PrefixNot);
            if (useNot) strMode = strMode.Substring(PrefixNot.Length);

            if (strMode == "default")
                strMode = "contains"; // 2017-11-18 old default was "default" - this is still in for compatibility

            if (!AllCompareModes.Contains(strMode))
                return (Error.Create(title: "CompareMode unknown", message: $"CompareMode other '{strMode}' is unknown."), "error");

            //if (!Enum.TryParse<CompareModes>(strMode, true, out var mode))
            //    return (SetError("CompareMode unknown", $"CompareMode other '{strMode}' is unknown."), "error");

            var childParent = ChildOrParent;
            if (!_directionPossibleValues.Contains(childParent, StringComparer.CurrentCultureIgnoreCase))
                return (Error.Create(title: "Can only compare Children", message: $"ATM can only find related children at the moment, must set {nameof(ChildOrParent)} to '{DefaultDirection}'"), "error");

            //var lang = Languages.ToLowerInvariant();
            //if (lang != "default")
            //	throw new Exception("Can't filter for languages other than 'default'");
            //if (lang == "default") lang = ""; // no language is automatically the default language

            var lowAttribName = compAttr.ToLowerInvariant();
            l.A($"get related on relationship:'{relationship}', filter:'{filter}', rel-field:'{compAttr}' mode:'{strMode}', child/parent:'{childParent}'");

            var source = TryGetIn();
            if (source is null) return (Error.TryGetInFailed(this), "error");

            var compType = lowAttribName == Attributes.EntityFieldAutoSelect
                ? CompareType.Auto
                : lowAttribName == Attributes.EntityFieldId
                    ? CompareType.Id
                    : lowAttribName == Attributes.EntityFieldTitle
                        ? CompareType.Title
                        : CompareType.Any;

            // pick the correct value-comparison
            Func<IEntity, string, bool> comparisonOnRelatedItem;
            if (compType == CompareType.Auto)
            {
                var getId = GetFieldValue(CompareType.Id, null);
                if (getId.IsError) return (getId.Errors, "error");
                var getTitle = GetFieldValue(CompareType.Title, null);
                if (getTitle.IsError) return (getTitle.Errors, "error");
                comparisonOnRelatedItem = CompareTwo(getId.Result, getTitle.Result);

            }
            else
            {
                var getValue = GetFieldValue(compType, compAttr);
                if (getValue.IsError) return (getValue.Errors, "error");
                comparisonOnRelatedItem = CompareOne(getValue.Result);
            }

            var filterList = Separator == DefaultSeparator
                ? new[] { filter }
                : filter.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);


            l.A($"will compare mode:{strMode} on:{compType} '{lowAttribName}', values to check ({filterList.Length}):'{filter}'");

            // pick the correct list-comparison - atm ca. 6 options
            var modeCompareOrError = PickMode(strMode, relationship, comparisonOnRelatedItem, filterList);
            if (modeCompareOrError.IsError) return (modeCompareOrError.Errors, "error");
            var modeCompare = modeCompareOrError.Result;

            var finalCompare = useNot
                ? e => !modeCompare(e)
                : modeCompare;

            try
            {
                var results = source.Where(finalCompare).ToImmutableList();

                return (results, $"{results.Count}");
            }
            catch (Exception ex)
            {
                return (Error.Create(title: "Error comparing Relationships", message: "Unknown error, check details in Insights logs", exception: ex), "error");
            }
        });


        /// <summary>
        /// Generate the condition (mode) which will be used, like contain-any, has-any, etc.
        /// </summary>
        /// <param name="modeToPick">mode-type</param>
        /// <param name="relationship">relationship on which the compare will operate</param>
        /// <param name="internalCompare">internal compare method</param>
        /// <param name="valuesToFind">value-list to compare to</param>
        /// <returns></returns>
        private ResultOrError<Func<IEntity, bool>> PickMode(string modeToPick, string relationship,
            Func<IEntity, string, bool> internalCompare, string[] valuesToFind) => Log.Func(l =>
        {
            switch (modeToPick)
            {
                case CompareModeContains:
                    if (valuesToFind.Length > 1)
                        return (new ResultOrError<Func<IEntity, bool>>(true,
                            entity =>
                            {
                                var rels = entity.Relationships.Children[relationship];
                                return valuesToFind.All(v => rels.Any(r => internalCompare(r, v)));
                            }), "contains all");
                    return (new ResultOrError<Func<IEntity, bool>>(true,
                        entity => entity.Relationships.Children[relationship]
                            .Any(r => internalCompare(r, valuesToFind.FirstOrDefault() ?? ""))
                    ), "contains one");
                case CompareModeContainsAny:
                    // Condition that of the needed relationships, at least one must exist
                    return (new ResultOrError<Func<IEntity, bool>>(true, entity =>
                    {
                        var rels = entity.Relationships.Children[relationship];
                        return valuesToFind.Any(v => rels.Any(r => internalCompare(r, v)));
                    }), "will use contains any");
                case CompareModeAny:
                    return (new ResultOrError<Func<IEntity, bool>>(true,
                        entity => entity.Relationships.Children[relationship].Any()), "will use any");
                case CompareModeFirst:
                    // Condition that of the needed relationships, the first must be what we want
                    return (new ResultOrError<Func<IEntity, bool>>(true, entity =>
                    {
                        var first = entity.Relationships.Children[relationship].FirstOrDefault();
                        return first != null && valuesToFind.Any(v => internalCompare(first, v));
                    }), "will use first is");
                case CompareModeCount:
                    // Count relationships
                    if (int.TryParse(valuesToFind.FirstOrDefault() ?? "0", out var count))
                        return (new ResultOrError<Func<IEntity, bool>>(true,
                            entity => entity.Relationships.Children[relationship].Count() == count), "count");

                    return (new ResultOrError<Func<IEntity, bool>>(true, _ => false), "count");

                default:
                    return (
                        new ResultOrError<Func<IEntity, bool>>(false, null,
                            Error.Create(source: this, title: "Mode unknown", message: $"The mode '{modeToPick}' is invalid")), "error, unknown compare mode");
                    //SetError("Mode unknown", $"The mode '{modeToPick}' is invalid");
                    //return (null, "error, unknown compare mode");
            }
        });



        private static Func<IEntity, string, bool> CompareTwo(Func<IEntity, string> getId, Func<IEntity, string> getTitle)
        {
            // in case the inner checks prepared an error, then the functions will be null and we need to forward this
            if (getId == null || getTitle == null) return null;
            return (entity, value) => getId(entity) == value || getTitle(entity) == value;
        }

        private static Func<IEntity, string, bool> CompareOne(Func<IEntity, string> getValue)
        {
            // in case the inner checks prepared an error, then the functions will be null and we need to forward this
            if (getValue == null) return null;
            return (entity, value) => getValue(entity) == value;
        }


        private ResultOrError<Func<IEntity, string>> GetFieldValue(CompareType type, string fieldName) => Log.Func(l =>
        {
            switch (type)
            {
                case CompareType.Any:
                    l.A($"compare on a normal attribute:{fieldName}");
                    return new ResultOrError<Func<IEntity, string>>(true, e =>
                    {
                        try
                        {
                            return e?[fieldName]?[0]?.ToString().ToLowerInvariant();
                        }
                        catch
                        {
                            // Note 2021-03-29 I think it's extremely unlikely that this will ever fire
                            throw new Exception("Error while trying to filter for related entities. " +
                                                "Probably comparing an attribute on the related entity that doesn't exist. " +
                                                $"Was trying to compare the attribute '{fieldName}'");
                        }
                    });
                case CompareType.Id:
                    l.A("will compare on ID");
                    return new ResultOrError<Func<IEntity, string>>(true, e => e?.EntityId.ToString());
                case CompareType.Title:
                    l.A("will compare on title");
                    return new ResultOrError<Func<IEntity, string>>(true, e => e?.GetBestTitle()?.ToLowerInvariant());
                // ReSharper disable once RedundantCaseLabel
                case CompareType.Auto:
                default:
                    return new ResultOrError<Func<IEntity, string>>(false, null, 
                        Error.Create(title: "Problem with CompareType", message: $"The CompareType '{type}' is unexpected."));
            }
        });

    }
}
