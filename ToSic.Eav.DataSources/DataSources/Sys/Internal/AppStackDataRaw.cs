using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Debug;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.DataSources.Sys.Internal;

// Note: ATM serves as Raw and as DTO, but should change soon.
// once we adjust the front end to use the query
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppStackDataRaw(PropertyDumpItem original) : RawEntityBase
{
    public const string TypeName = "AppStack";
    public const string TitleFieldName = Data.Attributes.TitleNiceName;

    public static DataFactoryOptions Options = new()
    {
        TitleField = TitleFieldName,
        TypeName = TypeName,
    };

    public string Source { get; set; } = original.SourceName;

    public int Priority { get; set; } = original.SourcePriority;

    public string Path { get; set; } = original.Path;

    public object Value { get; set; } = original.Property.Result;

    public string Type { get; set; } = original.Property.ValueType.ToString();

    public int TotalResults { get; set; } = original.AllOptions?.GroupBy(i => i.SourceName)?.Count() ?? 0; // do not count "duplicate" by SourceName

    public override IDictionary<string, object> Attributes(RawConvertOptions options)
    {
        var attributes = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
        {
            { Data.Attributes.TitleNiceName, Path },
            { nameof(Source), Source },
            { nameof(Priority), Priority },
            { nameof(Type), Type },
            { nameof(TotalResults), TotalResults },
        };
        if (options.ShouldAddKey(nameof(Value)))
            attributes[nameof(Value)] = Value;
        return attributes;
    }
}