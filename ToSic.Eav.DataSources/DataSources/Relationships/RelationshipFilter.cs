﻿using ToSic.Eav.Context;
using ToSic.Eav.DataSource.Internal.Errors;
using ToSic.Eav.DataSource.Streams.Internal;
using static ToSic.Eav.DataSource.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// Filter Entities by Value in a Related Entity. For example:
/// Find all Books (desired Entity), whose Authors (related Entity) have a Country (Attribute) with 'Switzerland' (Value). 
/// </summary>
[PublicApi]
[VisualQuery(
    NiceName = "Relationship Filter",
    UiHint = "Keep items having a relationship matching a criteria",
    Icon = DataSourceIcons.Share,
    Type = DataSourceType.Filter,
    NameId = "ToSic.Eav.DataSources.RelationshipFilter, ToSic.Eav.DataSources",
    In = [InStreamDefaultRequired, StreamFallbackName],
    DynamicOut = false,
    ConfigurationType = "|Config ToSic.Eav.DataSources.RelationshipFilter",
    HelpLink = "https://go.2sxc.org/DsRelationshipFilter")]
public sealed class RelationshipFilter : Eav.DataSource.DataSourceBase
{
    #region Configuration-properties

    [PrivateApi] internal const string FieldAttributeOnRelationship = "AttributeOnRelationship";
    [PrivateApi] internal const string FieldComparison = "Comparison";
    [PrivateApi] internal const string FieldDirection = "Direction";

    [PrivateApi] private const string PrefixNot = "not-";
    [PrivateApi] internal const string DefaultDirection = "child";
    [PrivateApi] private const string DefaultSeparator = "ignore"; // by default, don't separate!
    [PrivateApi] private readonly string[] _directionPossibleValues = [DefaultDirection];


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
    [PrivateApi] internal string[] AllCompareModes = [CompareModeContains, CompareModeContainsAny, CompareModeAny, CompareModeFirst, CompareModeCount
    ];


    private enum CompareType { Any, Id, Title, Auto }

    /// <summary>
    /// Relationship-attribute - in the example this would be 'Author' as we're checking values in related Author items. 
    /// </summary>
    [Configuration]
    public string Relationship
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }

    /// <summary>
    /// The filter-value that will be used - for example "Switzerland" when looking for authors from there
    /// </summary>
    [Configuration]
    public string Filter
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }

    /// <summary>
    /// The attribute we're looking into, in this case it would be 'Country' because we're checking what Authors are from Switzerland.
    /// </summary>
    [Configuration(Field = FieldAttributeOnRelationship, Fallback = Attributes.EntityFieldTitle)]
    public string CompareAttribute
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
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
        set => Configuration.SetThisObsolete(value.ToLowerInvariant());
    }

    /// <summary>
    /// Separator value where we have multiple values / IDs to compare. Default is 'ignore' = no separator
    /// </summary>
    [Configuration(Fallback = DefaultSeparator)]
    public string Separator
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value.ToLowerInvariant());
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
                throw new("Value '" + value + "'not allowed for ChildOrParent");
            Configuration.SetThisObsolete(valLower);
        }
    }

    #endregion

    /// <summary>
    /// Constructs a new RelationshipFilter
    /// </summary>
    [PrivateApi]
    public RelationshipFilter(MyServices services, IContextResolverUserPermissions userPermissions) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Relfil", connect: [userPermissions])
    {
        _userPermissions = userPermissions;
        ProvideOut(GetRelationshipsOrFallback);
        // todo: unclear if implemented...
        //ConfigMaskMyConfig(nameof(ChildOrParent), $"{Settings.Direction}||{DefaultDirection}");
    }
    private readonly IContextResolverUserPermissions _userPermissions;


    private IImmutableList<IEntity> GetRelationshipsOrFallback()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        var res = GetEntities();
        if (!res.Any() && In.HasStreamWithItems(StreamFallbackName))
            return l.Return(In[StreamFallbackName].List.ToImmutableList(), "fallback");

        return l.Return(res, "ok");
    }

    private IImmutableList<IEntity> GetEntities()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
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
            return l.ReturnAsError(Error.Create(title: "CompareMode unknown", message: $"CompareMode other '{strMode}' is unknown."));

        //if (!Enum.TryParse<CompareModes>(strMode, true, out var mode))
        //    return (SetError("CompareMode unknown", $"CompareMode other '{strMode}' is unknown."), "error");

        var childParent = ChildOrParent;
        if (!_directionPossibleValues.Contains(childParent, StringComparer.CurrentCultureIgnoreCase))
            return l.ReturnAsError(Error.Create(title: "Can only compare Children", message: $"ATM can only find related children at the moment, must set {nameof(ChildOrParent)} to '{DefaultDirection}'"));

        //var lang = Languages.ToLowerInvariant();
        //if (lang != "default")
        //	throw new Exception("Can't filter for languages other than 'default'");
        //if (lang == "default") lang = ""; // no language is automatically the default language

        var lowAttribName = compAttr.ToLowerInvariant();
        l.A($"get related on relationship:'{relationship}', filter:'{filter}', rel-field:'{compAttr}' mode:'{strMode}', child/parent:'{childParent}'");

        var source = TryGetIn();
        if (source is null)
            return l.ReturnAsError(Error.TryGetInFailed());

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
            if (getId.IsError)
                return l.ReturnAsError(getId.Errors);
            var getTitle = GetFieldValue(CompareType.Title, null);
            if (getTitle.IsError)
                return l.ReturnAsError(getTitle.Errors);
            comparisonOnRelatedItem = CompareTwo(getId.Result, getTitle.Result);

        }
        else
        {
            var getValue = GetFieldValue(compType, compAttr);
            if (getValue.IsError)
                return l.ReturnAsError(getValue.Errors);
            comparisonOnRelatedItem = CompareOne(getValue.Result);
        }

        var filterList = Separator == DefaultSeparator
            ? [filter]
            : filter.Split([Separator], StringSplitOptions.RemoveEmptyEntries);


        l.A($"will compare mode:{strMode} on:{compType} '{lowAttribName}', values to check ({filterList.Length}):'{filter}'");

        // pick the correct list-comparison - atm ca. 6 options
        var modeCompareOrError = PickMode(strMode, relationship, comparisonOnRelatedItem, filterList);
        if (modeCompareOrError.IsError)
            return l.ReturnAsError(modeCompareOrError.Errors);
        var modeCompare = modeCompareOrError.Result;

        var finalCompare = useNot
            ? e => !modeCompare(e)
            : modeCompare;

        try
        {
            var selection = source.Where(finalCompare).ToList();

            var results = selection.ToImmutableList();

            return l.Return(results, $"{results.Count}");
        }
        catch (Exception ex)
        {
            return l.ReturnAsError(Error.Create(title: "Error comparing Relationships", message: "Unknown error, check details in Insights logs", exception: ex));
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
    private ResultOrError<Func<IEntity, bool>> PickMode(string modeToPick, string relationship, Func<IEntity, string, bool> internalCompare, string[] valuesToFind)
    {
        var l = Log.Fn<ResultOrError<Func<IEntity, bool>>>();
        switch (modeToPick)
        {
            case CompareModeContains:
                if (valuesToFind.Length > 1)
                    return l.Return(new(true, entity =>
                        {
                            var rels = entity.Relationships.Children[relationship];
                            return valuesToFind.All(v => rels.Any(r => internalCompare(r, v)));
                        }),
                        "contains all");
                return l.Return(new(true, entity => entity.Relationships.Children[relationship]
                            .Any(r => internalCompare(r, valuesToFind.FirstOrDefault() ?? ""))
                    ),
                    "contains one");

            case CompareModeContainsAny:
                // Condition that of the needed relationships, at least one must exist
                return l.Return(new(true, entity =>
                    {
                        var rels = entity.Relationships.Children[relationship];
                        return valuesToFind.Any(v => rels.Any(r => internalCompare(r, v)));
                    }),
                    "will use contains any");

            case CompareModeAny:
                return l.Return(new(true,
                    entity => entity.Relationships.Children[relationship].Any()), 
                    "will use any");

            case CompareModeFirst:
                // Condition that of the needed relationships, the first must be what we want
                return l.Return(new(true, entity =>
                    {
                        var first = entity.Relationships.Children[relationship].FirstOrDefault();
                        return first != null && valuesToFind.Any(v => internalCompare(first, v));
                    }),
                    "will use first is");

            case CompareModeCount:
                // Count relationships
                if (int.TryParse(valuesToFind.FirstOrDefault() ?? "0", out var count))
                    return l.Return(new(true,
                            entity => entity.Relationships.Children[relationship].Count() == count),
                        "count");

                return l.Return(new(true, _ => false), "count");

            default:
                return l.Return(new(false, null, Error.Create(source: this, title: "Mode unknown",
                            message: $"The mode '{modeToPick}' is invalid")),
                    "error, unknown compare mode");
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


    private ResultOrError<Func<IEntity, string>> GetFieldValue(CompareType type, string fieldName) => Log.Func(l =>
    {
        switch (type)
        {
            case CompareType.Any:
                l.A($"compare on a normal attribute:{fieldName}");
                return new(true, e =>
                {
                    try
                    {
                        return e?[fieldName]?[0]?.ToString().ToLowerInvariant();
                    }
                    catch
                    {
                        // Note 2021-03-29 I think it's extremely unlikely that this will ever fire
                        throw new("Error while trying to filter for related entities. " +
                                  "Probably comparing an attribute on the related entity that doesn't exist. " +
                                  $"Was trying to compare the attribute '{fieldName}'");
                    }
                });
            case CompareType.Id:
                l.A("will compare on ID");
                return new(true, e => e?.EntityId.ToString());
            case CompareType.Title:
                l.A("will compare on title");
                return new(true, e => e?.GetBestTitle()?.ToLowerInvariant());
            // ReSharper disable once RedundantCaseLabel
            case CompareType.Auto:
            default:
                return new ResultOrError<Func<IEntity, string>>(false, null, 
                    Error.Create(title: "Problem with CompareType", message: $"The CompareType '{type}' is unexpected."));
        }
    });

}