namespace ToSic.Eav.WebApi.Routing;

/// <summary>
/// These are the keys / names of fields we use in rout parts
/// </summary>
public class VarNames
{
    public const string Edition = "edition";
    public const string Controller = "controller";
    public const string Action = "action";
    public const string Name = "name";
    public const string Stream = "stream";
    public const string AppPath = "apppath";
    public const string ContentType = "contenttype";
    public const string Id = "id";
    public const string Guid = "guid";
    public const string Field = "field";


    // todo: not quite the right place, but deduplicating dnn/oqtane code
    public static string GetEdition<TValue>(IDictionary<string, TValue> routeValues)
    {
        var edition = routeValues.TryGetValue(VarNames.Edition, out var value)
            ? value?.ToString() ?? ""
            : "";
        return edition + (string.IsNullOrEmpty(edition) ? "" : "/");
    }

}