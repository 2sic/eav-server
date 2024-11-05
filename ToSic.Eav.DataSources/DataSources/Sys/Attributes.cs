using System.Text.RegularExpressions;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Internal;
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
    public Attributes(IAppReaderFactory appReaders, MyServices services, IDataFactory dataFactory) : base(services, $"{LogPrefix}.Attrib", connect: [appReaders, dataFactory])
    {
        _appReaders = appReaders;
        _dataFactory = dataFactory.New(options: new(typeName: AttribContentTypeName, titleField: nameof(IAttributeType.Title)));

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
                    contentTypeName: at.Type.Name,
                    // TODO: FILTER html
                    description: at.Attribute.Metadata.GetBestValue<string>(AttributeMetadata.DescriptionField)
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
                            contentTypeName: "dynamic",
                            description: "dynamic"
                        )
                    )
                    .ToList()
                : null;

        // No attributes, exit early
        if (list == null)
            return l.Return(EmptyList, "no attributes found");

        // Create short list of all fields for use in finding additional system fields
        var foundFieldNames = list
            .Select(dic => dic[nameof(IAttributeType.Name)] as string)
            .Where(x => x != null)
            .ToList();

        // Add descriptions of system fields such as Id, Created, Modified etc.
        var sysFields = GetSystemFields(types, foundFieldNames);
        // ...in reverse order, so ID etc. are on top in the end...
        list = sysFields.Concat(list).ToList();

        // if it didn't work yet, maybe try from stream items
        var data = list.Select(attribData => _dataFactory.Create(attribData)).ToImmutableList();
        return l.Return(data, $"{data.Count}");
    }

    private static IEnumerable<Dictionary<string, object>> GetSystemFields(List<IContentType> types, List<string> foundFieldNames)
    {
        // New 2022-10-17 2dm - Add System fields such as Id, Created, Modified etc.
        // But only if they weren't already added by the content type, so if the ContentType had an "Id" field, we shouldn't override it here.
        var sysFieldsWhichWereNotAdded = Data.Attributes.SystemFields
            .Where(sysField => !foundFieldNames.Any(f => f.EqualsInsensitive(sysField.Key)))
            .ToList();

        // Find possible metadata for system fields.
        // This is mainly for system types like AppAssets which may want to tell the user that ID shouldn't be used
        var sysFieldAttributes = types
            .Select(t => t.GetDecorator<ContentTypeVirtualAttributes>())
            .Where(x => x != null)
            .SelectMany(x => x.VirtualAttributes)
            .ToList();

        var additions = sysFieldsWhichWereNotAdded
            .Select(sysField =>
            {
                var descriptionProvider = sysFieldAttributes
                    .FirstOrDefault(x => x.Key == sysField.Key).Value;
                var description = descriptionProvider?.Metadata.GetBestValue<string>(AttributeMetadata.DescriptionField)
                                  ?? (Data.Attributes.SystemFieldDescriptions.TryGetValue(sysField.Key, out var desc)
                                      ? desc
                                      : default);
                return AsDic(
                    sysField.Key,
                    ValueTypeHelpers.Get(sysField.Value),
                    false,
                    0,
                    true,
                    "all",
                    description: description
                );
            })
            .ToList();
        return additions;
    }

    private static Dictionary<string, object> AsDic(
        string name,
        ValueTypes type,
        bool isTitle,
        int sortOrder,
        bool builtIn,
        string contentTypeName,
        string description = default
    ) => new()
        {
            [nameof(IAttributeType.Name)] = name,
            [nameof(IAttributeType.Type)] = type.ToString(),
            [nameof(IAttributeType.IsTitle)] = isTitle,
            [nameof(IAttributeType.SortOrder)] = sortOrder,
            [nameof(IAttributeType.IsBuiltIn)] = builtIn,
            [nameof(IAttributeType.Title)] = $"{name} ({type}{(builtIn ? ", built-in" : "")})",
            [nameof(IAttributeType.ContentType)] = contentTypeName,
            [nameof(IAttributeType.Description)] = CleanDescription(description),
        };

    /// <summary>
    /// Note: this could be done better with RazorBlade, but ATM we don't want to add dependencies just for this.
    /// If we ever do add RazorBlade, then we should also correct &nbsp; etc.
    /// </summary>
    /// <param name="html"></param>
    /// <returns></returns>
    private static string CleanDescription(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return html;

        html = html.Replace("<br>", "\n");
        var clean = StripHtml(html);
        var enter = clean.IndexOf("\n", StringComparison.Ordinal);
        var firstLine = enter > 0
            ? clean.Substring(0, enter -1)
            : clean;

        return firstLine.Length > 100
            ? firstLine.Substring(0, 100) + "..."
            : firstLine;
    }

    private static string StripHtml(string html) => string.IsNullOrWhiteSpace(html) ? html : Regex.Replace(html, "<.*?>", string.Empty);
}