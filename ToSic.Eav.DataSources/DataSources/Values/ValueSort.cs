using ToSic.Eav.Data.Sys;
using ToSic.Eav.DataSources.Internal;
using static ToSic.Eav.DataSource.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// Sort Entity by values in specified Attributes / Properties
/// </summary>
[PublicApi]
[VisualQuery(
    NiceName = "Value Sort",
    UiHint = "Sort items by a property",
    Icon = DataSourceIcons.Sort,
    Type = DataSourceType.Sort,
    NameId = "ToSic.Eav.DataSources.ValueSort, ToSic.Eav.DataSources",
    DynamicOut = false,
    In = [InStreamDefaultRequired],
    ConfigurationType = "|Config ToSic.Eav.DataSources.ValueSort",
    HelpLink = "https://go.2sxc.org/DsValueSort")]

public sealed class ValueSort : Eav.DataSource.DataSourceBase
{
    #region Configuration-properties
        
    /// <summary>
    /// The attribute whose value will be sorted by.
    /// </summary>
    [Configuration]
    public string Attributes
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }

    /// <summary>
    /// The sorting direction like 'asc' or 'desc', can also be 0, 1
    /// </summary>
    [Configuration]
    public string Directions
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }

    /// <summary>
    /// Language to filter for. At the moment it is not used, or it is trying to find "any"
    /// </summary>
    [Configuration(Fallback = ValueLanguages.LanguageDefaultPlaceholder)]
    public string Languages
    {
        get => Configuration.GetThis();
        set => Configuration.SetThisObsolete(value);
    }
    #endregion

    /// <inheritdoc />
    /// <summary>
    /// Constructs a new ValueSort
    /// </summary>
    [PrivateApi]
    public ValueSort(ValueLanguages valLanguages, MyServices services) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.ValSrt")
    {
        ConnectLogs([
            _valLanguages = valLanguages
        ]);

        ProvideOut(GetValueSort);
    }
    private readonly ValueLanguages _valLanguages;

    /// <summary>
    /// The internal language list used to lookup values.
    /// It's internal, to allow testing/debugging from the outside
    /// </summary>
    [PrivateApi] internal string[] LanguageList { get; private set; }

    private const char FieldId = 'i';
    private const char FieldMod = 'm';
    private const char FieldTitle = 't';
    private const char FieldCreate = 'c';
    private const char FieldNormal = 'x';

    private IImmutableList<IEntity> GetValueSort()
    {
        // todo: maybe do something about languages?
        // todo: test decimal / number types

        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();

        l.A("will apply value-sort");
        var sortAttributes = Attributes.CsvToArrayWithoutEmpty();
        var sortDirections = Directions.CsvToArrayWithoutEmpty();
        var descendingCodes = new[] { "desc","d","0",">" };

        // Languages check - not fully implemented yet, only supports "default" / "current"
        LanguageList = _valLanguages.PrepareLanguageList(Languages);

        var source = TryGetIn();
        if (source is null) return l.ReturnAsError(Error.TryGetInFailed());

        // check if no list parameters specified
        if (sortAttributes.Length == 0 || (sortAttributes.Length == 1 && string.IsNullOrWhiteSpace(sortAttributes[0])))
            return l.Return(source, "no params");

        // if list is blank, then it didn't find the attribute to sort by - so just return unsorted
        // note 2020-10-07 this may have been a bug previously, returning an empty list instead
        if (!source.Any()) return l.Return(source, "sort-attribute not found in data");

        // Keep entities which cannot sort by the required values (removed previously from results)
        //var unsortable = originals.Where(e => !results.Contains(e)).ToImmutableArray();

        IOrderedEnumerable<IEntity> ordered = null;

        for (var i = 0; i < sortAttributes.Length; i++)
        {
            // get attribute-name and type; set type=id|title for special cases
            var a = sortAttributes[i];
            var aLow = a.ToLowerInvariant();
            var specAttr = aLow == AttributeNames.EntityFieldId ? FieldId
                : aLow == AttributeNames.EntityFieldTitle ? FieldTitle 
                : aLow == AttributeNames.EntityFieldModified ? FieldMod
                : aLow == AttributeNames.EntityFieldCreated ? FieldCreate
                : FieldNormal;
            var isAscending = true;			// default
            if (sortDirections.Length - 1 >= i)	// if this value has a direction specified, use that...
                isAscending = !descendingCodes.Any(sortDirections[i].ToLowerInvariant().Trim().Contains);

            var getValue = GetPropertyToSortFunc(specAttr, a, LanguageList);

            ordered = ordered == null
                // First sort - no ordered data yet
                ? isAscending ? source.OrderBy(getValue) : source.OrderByDescending(getValue)
                // Following sorts, extend previous sort
                : isAscending ? ordered.ThenBy(getValue) : ordered.ThenByDescending(getValue);
        }

        IImmutableList<IEntity> final;
        try
        {
            final = ordered?.ToImmutableList() ?? [];
        }
        catch (Exception e)
        {
            return l.ReturnAsError(Error.Create(title: "Error sorting", message: "Sorting failed - see exception in insights", exception: e));
        }

        return l.ReturnAsOk(final);
    }

    private static Func<IEntity, object> GetPropertyToSortFunc(char propertyCode, string fieldName, string[] languages)
        => propertyCode switch
        {
            FieldId => e => e.EntityId,
            FieldMod => e => e.Modified,
            FieldCreate => e => e.Created,
            FieldTitle => e => e.GetBestTitle(languages),
            _ => e => e.Get(fieldName, languages: languages)
        };
}