using System.Drawing.Printing;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.PropertyDump;

namespace ToSic.Eav.DataSources.Sys;

// Note: ATM serves as Raw and as DTO, but should change soon.
// once we adjust the front end to use the query
[ShowApiWhenReleased(ShowApiMode.Never)]
[ContentType(
    Guid = "8b668008-c6f9-47e5-a5b7-cb9b521192c3",
    Description = "App Stack Value Information",
    Name = TypeName
)]
public class AppStackDataRaw(PropertyDumpItem original) : IRawEntityConvertible
{
    public const string TypeName = "AppStack";

    public static DataFactoryOptions Options = new()
    {
        TypeName = TypeName,
    };

    public string Source { get; set; } = original.SourceName;

    public int Priority { get; set; } = original.SourcePriority;

    [ContentTypeField(IsTitle = true)]
    public string Path { get; set; } = original.Path;

    public object? Value { get; set; } = original.Property?.Result;

    public string Type { get; set; } = original.Property?.ValueType.ToString() ?? nameof(ValueTypes.Undefined);

    public int TotalResults { get; set; } = original.AllOptions
                                                ?.GroupBy(i => i.SourceName)
                                                .Count()
                                            ?? 0; // do not count "duplicate" by SourceName


    IRawEntityConverter IRawEntityConvertible.GetConverter() => Converter;

    private static IRawEntityConverter Converter { get; } = new RawEntityConverterFactory<AppStackDataRaw>((source, options) =>
        new RawEntity
        {
            Values = GetValues(source, options)
        });

    private static IDictionary<string, object?> GetValues(AppStackDataRaw data, RawConvertOptions options)
    {
        var attributes = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase)
        {
            { AttributeNames.TitleNiceName, data.Path },
            { nameof(Path), data.Path },
            { nameof(Source), data.Source },
            { nameof(Priority), data.Priority },
            { nameof(Type), data.Type },
            { nameof(TotalResults), data.TotalResults },
        };
        if (options.ShouldAddKey(nameof(Value)))
            attributes[nameof(Value)] = data.Value;
        return attributes;
    }
}
