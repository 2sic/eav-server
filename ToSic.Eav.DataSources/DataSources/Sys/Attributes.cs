using ToSic.Eav.Apps;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSources.Sys.Types;
using ToSic.Eav.Plumbing;
using static ToSic.Eav.DataSource.Internal.DataSourceConstants;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources.Sys;

/// <inheritdoc />
/// <summary>
/// A DataSource that returns the attributes of a content-type
/// </summary>
/// <remarks>
/// * Changed in v15.05 to use the [immutable convention](xref:NetCode.Conventions.Immutable)
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[VisualQuery(
    NiceName = "Attributes of Type",
    UiHint = "Attributes/fields of a Content-Type",
    Icon = DataSourceIcons.Dns,
    Type = DataSourceType.System,
    NameId = "ToSic.Eav.DataSources.System.Attributes, ToSic.Eav.DataSources",
    Audience = Audience.Advanced,
    DynamicOut = false,
    ConfigurationType = "5461d34d-7dc6-4d38-9250-a0729cc8ead3",
    HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Attributes")]

public sealed class Attributes: DataSourceBase
{

    #region Configuration-properties (no config)

    private const string TypeNameFallbackToTryToUseInStream = "not-configured-try-in"; // can't be blank, otherwise tokens fail
    private const string AttribContentTypeName = "Attribute";
	    
    /// <summary>
    /// The content-type name
    /// </summary>
    [Configuration(Fallback = TypeNameFallbackToTryToUseInStream)]
    public string ContentTypeName => Configuration.GetThis();
        
    #endregion

    /// <inheritdoc />
    /// <summary>
    /// Constructs a new Attributes DS
    /// </summary>
    public Attributes(IAppReaderFactory appReaders, MyServices services, IDataFactory dataFactory) : base(services, $"{LogPrefix}.Attrib")
    {
        ConnectLogs([
            _appReaders = appReaders,
            _dataFactory = dataFactory.New(options: new(typeName: AttribContentTypeName, titleField: nameof(IAttributeType.Title)))
        ]);
        ProvideOut(GetList);
    }
    private readonly IAppReaderFactory _appReaders;
    private readonly IDataFactory _dataFactory;

    private IImmutableList<IEntity> GetList()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();

        // try to load the content-type - if it fails, return empty list
        if (string.IsNullOrWhiteSpace(ContentTypeName))
            return l.Return(EmptyList, "no type name");

        var typeNames = ContentTypeName.CsvToArrayWithoutEmpty();
        if (typeNames.Length == 0)
            return l.Return(EmptyList, "no type names");

        var useStream = TypeNameFallbackToTryToUseInStream == ContentTypeName && In.ContainsKey(StreamDefaultName);
        var optionalList = useStream
            ? In[StreamDefaultName]?.List.ToImmutableList()
            : null;

        var appReader = _appReaders.Get(this);
        var firstEntityInStream = useStream ? optionalList?.FirstOrDefault() : null;
        var types = useStream 
            ? (firstEntityInStream?.Type).ToListOfOne()
            : typeNames.Select(appReader.GetContentType).ToList();


        if (!types.Any())
            return l.Return(EmptyList, "no type found");

        // try to load from type, if it exists
        var attributes = types
            .SelectMany(t =>
                t.Attributes?.Select(a => new
                {
                    Type = t,
                    Attribute = a,
                    a.Name,
                })
                ?? []
            )
            .DistinctBy(set => set.Name)
            .OrderBy(at => at.Name)
            .ToList();


        // todo: when supporting multiple types, consider adding more info what type they are from
        var list = attributes
            .Select(at => AsDic(
                    name: at.Name,
                    type: at.Attribute.Type,
                    isTitle: at.Attribute.IsTitle,
                    sortOrder: at.Attribute.SortOrder,
                    builtIn: false,
                    contentTypeName: at.Type.Name
                )
            )
            .ToList();

        // note that often dynamic types will have zero attributes, so the previous command
        // gives a 0-list of attributes
        // so if that's the case, check the first item in the results
        if (list.SafeNone())
            list = TypeNameFallbackToTryToUseInStream == ContentTypeName
                ? firstEntityInStream?.Attributes
                    .OrderBy(atPair => atPair.Key)
                    .Select(atPair => AsDic(
                            name: atPair.Key,
                            type: atPair.Value.Type,
                            isTitle: false,
                            sortOrder: 0,
                            builtIn: false,
                            contentTypeName: "dynamic"
                        )
                    )
                    .ToList()
                : null;

        if (list == null)
            return l.Return(EmptyList, "no attributes found");

        // New 2022-10-17 2dm - also add Id, Created, Modified etc.
        foreach (var sysField in Data.Attributes.SystemFields.Reverse())
            if (!list.Any(dic =>
                    dic.TryGetValue(nameof(IAttributeType.Name), out var name) &&
                    name as string == sysField.Key))
                list.Insert(0, AsDic(sysField.Key, ValueTypeHelpers.Get(sysField.Value), false, 0, true, "all"));

        // if it didn't work yet, maybe try from stream items
        var data = list.Select(attribData => _dataFactory.Create(attribData)).ToImmutableList();
        return l.Return(data, $"{data.Count}");
    }

    private static Dictionary<string, object> AsDic(
        string name,
        ValueTypes type,
        bool isTitle,
        int sortOrder,
        bool builtIn,
        string contentTypeName)
        => new()
        {
            [nameof(IAttributeType.Name)] = name,
            [nameof(IAttributeType.Type)] = type.ToString(),
            [nameof(IAttributeType.IsTitle)] = isTitle,
            [nameof(IAttributeType.SortOrder)] = sortOrder,
            [nameof(IAttributeType.IsBuiltIn)] = builtIn,
            [nameof(IAttributeType.Title)] = $"{name} ({type}{(builtIn ? ", built-in" : "")})",
            [nameof(IAttributeType.ContentType)] = contentTypeName,
        };
}