using System;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
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
        Icon = "share",
        Type = DataSourceType.Filter,
        GlobalName = "ToSic.Eav.DataSources.RelationshipFilter, ToSic.Eav.DataSources",
        In = new[] { Constants.DefaultStreamNameRequired, Constants.FallbackStreamName },
        DynamicOut = false,
        ExpectsDataOfType = "|Config ToSic.Eav.DataSources.RelationshipFilter",
        HelpLink = "https://r.2sxc.org/DsRelationshipFilter")]
    public sealed class RelationshipFilter : DataSourceBase
    {
        #region Configuration-properties
        /// <inheritdoc/>
        [PrivateApi]
        public override string LogId => $"{DataSourceConstants.LogPrefix}.RelFil";

        /// <summary>
        /// Settings-keys as they are used in the entity which provides settings
        /// </summary>
        /// <remarks>
        /// Don't change these terms, the spelling etc. must stay exactly like this
        /// </remarks>
        public enum Settings
        {
            Relationship,
            Filter,
            AttributeOnRelationship,
            Comparison,
            Direction, // important: not surfaced yet to the outside world as not implemented
            Separator
        }

        private const string PrefixNot = "not-";
        private const string RelationshipKey = "Relationship";
        private const string FilterKey = "Filter";
        private const string CompareAttributeKey = "CompareAttribute";
        private const string CompareModeKey = "Mode";
        private const string SeparatorKey = "Separator";
        private const string ChildOrParentKey = "ChildOrParent";
        private const string DefaultDirection = "child";
        private const string DefaultSeparator = "ignore"; // by default, don't separate!
        private readonly string[] _directionPossibleValues = { DefaultDirection };


        // ReSharper disable InconsistentNaming
        // ReSharper disable IdentifierTypo
        // this must all be in lower-case, to make further case-changes irrelevant
        public enum CompareModes
        {
            contains, // default case - must contain all provided filter-values
            containsany, // must contain any of the provided filter-values
            any, // must contain anything (not empty)
            first, // first item must match provided filter-values
            count
        }
        // ReSharper restore IdentifierTypo
        // ReSharper restore InconsistentNaming

        private enum CompareType { Any, Id, Title, Auto }

        /// <summary>
        /// Relationship-attribute - in the example this would be 'Author' as we're checking values in related Author items. 
        /// </summary>
        public string Relationship
        {
            get => Configuration[RelationshipKey];
            set => Configuration[RelationshipKey] = value;
        }

        /// <summary>
        /// The filter-value that will be used - for example "Switzerland" when looking for authors from there
        /// </summary>
        public string Filter
        {
            get => Configuration[FilterKey];
            set => Configuration[FilterKey] = value;
        }

        /// <summary>
        /// The attribute we're looking into, in this case it would be 'Country' because we're checking what Authors are from Switzerland.
        /// </summary>
		public string CompareAttribute
        {
            get => Configuration[CompareAttributeKey];
            set => Configuration[CompareAttributeKey] = value;
        }

        /// <summary>
        /// Comparison mode.
        /// "default" and "contains" will check if such a relationship is available
        /// other modes like "equals" or "exclude" not implemented
        /// </summary>
        public string CompareMode
        {
            get => Configuration[CompareModeKey];
            set => Configuration[CompareModeKey] = value.ToLowerInvariant();
        }

        /// <summary>
        /// Separator value where we have multiple values / IDs to compare. Default is 'ignore' = no separator
        /// </summary>
		public string Separator
        {
            get => Configuration[SeparatorKey];
            set => Configuration[SeparatorKey] = value.ToLowerInvariant();
        }

        /// <summary>
        /// Determines if the relationship we're looking into is a 'child'-relationship (default) or 'parent' relationship.
        /// </summary>
		public string ChildOrParent
        {
            get => Configuration[ChildOrParentKey];
            set
            {
                if (_directionPossibleValues.Contains(value.ToLowerInvariant()))
                    Configuration[ChildOrParentKey] = value.ToLowerInvariant();
                else
                    throw new Exception("Value '" + value + "'not allowed for ChildOrParent");
            }
        }

        #endregion

        /// <summary>
        /// Constructs a new RelationshipFilter
        /// </summary>
        [PrivateApi]
        public RelationshipFilter()
        {
            Provide(GetRelationshipsOrFallback);
            ConfigMask(RelationshipKey, $"[Settings:{Settings.Relationship}]");
            ConfigMask(FilterKey, $"[Settings:{Settings.Filter}]");
            ConfigMask(CompareAttributeKey, $"[Settings:{Settings.AttributeOnRelationship}||{Attributes.EntityFieldTitle}]");
            ConfigMask(CompareModeKey, $"[Settings:{Settings.Comparison}||{CompareModes.contains}]");
            ConfigMask(SeparatorKey, $"[Settings:{Settings.Separator}||{DefaultSeparator}]");
            ConfigMask(ChildOrParentKey, $"[Settings:{Settings.Direction}||{DefaultDirection}]");
        }

        private IImmutableList<IEntity> GetRelationshipsOrFallback()
        {
            var wrapLog = Log.Fn<IImmutableList<IEntity>>();
            var res = GetEntities();
            if (!res.Any() && In.HasStreamWithItems(Constants.FallbackStreamName))
                return wrapLog.Return(In[Constants.FallbackStreamName].List.ToImmutableList(), "fallback");

            return wrapLog.ReturnAsOk(res);
        }

        private IImmutableList<IEntity> GetEntities()
        {
            // todo: maybe do something about languages on properties?

            var wrapLog = Log.Fn<IImmutableList<IEntity>>();

            Configuration.Parse();

            var relationship = Relationship;
            var compAttr = CompareAttribute;
            var filter = Filter.ToLowerInvariant(); // new: make case insensitive
            var strMode = CompareMode.ToLowerInvariant();
            var useNot = strMode.StartsWith(PrefixNot);
            if (useNot) strMode = strMode.Substring(PrefixNot.Length);

            if (strMode == "default") strMode = "contains"; // 2017-11-18 old default was "default" - this is still in for compatibility
            if (!Enum.TryParse<CompareModes>(strMode, true, out var mode))
                return wrapLog.Return(SetError("CompareMode unknown", $"CompareMode other '{strMode}' is unknown."),
                    "error");

            var childParent = ChildOrParent;
            if (!_directionPossibleValues.Contains(childParent, StringComparer.CurrentCultureIgnoreCase))
                return wrapLog.Return(SetError("Can only compare Children",
                        $"ATM can only find related children at the moment, must set {nameof(ChildOrParent)} to '{DefaultDirection}'"),
                    "error");

            //var lang = Languages.ToLowerInvariant();
            //if (lang != "default")
            //	throw new Exception("Can't filter for languages other than 'default'");
            //if (lang == "default") lang = ""; // no language is automatically the default language

            var lowAttribName = compAttr.ToLowerInvariant();
            Log.A($"get related on relationship:'{relationship}', filter:'{filter}', rel-field:'{compAttr}' mode:'{mode}', child/parent:'{childParent}'");

            if (!GetRequiredInList(out var originals))
                return wrapLog.Return(originals, "error");

            var compType = lowAttribName == Attributes.EntityFieldAutoSelect
                ? CompareType.Auto
                : lowAttribName == Attributes.EntityFieldId
                    ? CompareType.Id
                    : lowAttribName == Attributes.EntityFieldTitle
                        ? CompareType.Title
                        : CompareType.Any;

            // pick the correct value-comparison
            var comparisonOnRelatedItem = compType == CompareType.Auto
                ? CompareTwo(GetFieldValue(CompareType.Id, null), GetFieldValue(CompareType.Title, null))
                : CompareOne(GetFieldValue(compType, compAttr));

            if (comparisonOnRelatedItem == null)
                return wrapLog.Return(ErrorStream, "error");

            var filterList = Separator == DefaultSeparator
                ? new[] { filter }
                : filter.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries);


            Log.A($"will compare mode:{mode} on:{compType} '{lowAttribName}', values to check ({filterList.Length}):'{filter}'");

            // pick the correct list-comparison - atm ca. 6 options
            var modeCompare = PickMode(mode, relationship, comparisonOnRelatedItem, filterList);
            if (modeCompare == null)
                return wrapLog.Return(ErrorStream, "error");

            var finalCompare = useNot
                ? e => !modeCompare(e)
                : modeCompare;

            try
            {
                var results = originals.Where(finalCompare).ToImmutableArray();

                return wrapLog.Return(results, $"{results.Length}");
            }
            catch (Exception ex)
            {
                return wrapLog.Return(SetError("Error comparing Relationships", "Unknown error, check details in Insights logs", ex), "error");
            }
        }


        /// <summary>
        /// Generate the condition (mode) which will be used, like contain-any, has-any, etc.
        /// </summary>
        /// <param name="modeToPick">mode-type</param>
        /// <param name="relationship">relationship on which the compare will operate</param>
        /// <param name="internalCompare">internal compare method</param>
        /// <param name="valuesToFind">value-list to compare to</param>
        /// <returns></returns>
	    private Func<IEntity, bool> PickMode(CompareModes modeToPick, string relationship, Func<IEntity, string, bool> internalCompare, string[] valuesToFind)
        {
            switch (modeToPick)
            {
                case CompareModes.contains:
                    Log.A("will use contains one / all");
                    if (valuesToFind.Length > 1)
                        return entity =>
                        {
                            var rels = entity.Relationships.Children[relationship];
                            return valuesToFind.All(v => rels.Any(r => internalCompare(r, v)));
                        };
                    else
                        return entity => entity.Relationships.Children[relationship]
                            .Any(r => internalCompare(r, valuesToFind.FirstOrDefault() ?? ""));
                case CompareModes.containsany: // Condition that of the needed relationships, at least one must exist
                    Log.A("will use contains any");
                    return entity =>
                    {
                        var rels = entity.Relationships.Children[relationship];
                        return valuesToFind.Any(v => rels.Any(r => internalCompare(r, v)));
                    };
                case CompareModes.any:
                    Log.A("will use has-any");
                    return entity => entity.Relationships.Children[relationship].Any();
                case CompareModes.first:
                    // Condition that of the needed relationships, the first must be what we want
                    Log.A("will use first is");
                    return entity =>
                    {
                        var first = entity.Relationships.Children[relationship].FirstOrDefault();
                        return first != null && valuesToFind.Any(v => internalCompare(first, v));
                    };
                case CompareModes.count:
                    Log.A("will use count");
                    if (int.TryParse(valuesToFind.FirstOrDefault() ?? "0", out int count))
                        return entity => entity.Relationships.Children[relationship].Count() == count;

                    return entity => false;

                default:
                    SetError("Mode unknown", $"The mode '{modeToPick}' is invalid");
                    return null;
            }
        }



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


        private Func<IEntity, string> GetFieldValue(CompareType type, string fieldName)
        {
            switch (type)
            {
                case CompareType.Any:
                    Log.A($"will compare on a normal attribute:{fieldName}");
                    return e =>
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
                    };
                case CompareType.Id:
                    Log.A("will compare on ID");
                    return e => e?.EntityId.ToString();
                case CompareType.Title:
                    Log.A("will compare on title");
                    return e => e?.GetBestTitle()?.ToLowerInvariant();
                // 2021-03-29
                //return e => e?.Title?[0]?.ToString().ToLowerInvariant();
                // ReSharper disable once RedundantCaseLabel
                case CompareType.Auto:
                default:
                    SetError("Problem with CompareType", $"The CompareType '{type}' is unexpected.");
                    return null;
            }
        }

    }
}
