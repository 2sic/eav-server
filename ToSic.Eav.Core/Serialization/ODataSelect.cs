namespace ToSic.Eav.Serialization;

/// <summary>
/// Create a new EntitySerializationDecorator based on the $select parameters in the URL to filter the fields
/// </summary>
/// <param name="rawFields">list of fields to select</param>
/// <returns></returns>
[PrivateApi]
public class ODataSelect(List<string>? rawFields)
{
    public const string Main = "main";

    public Dictionary<string, List<string>> FieldsByName => field
        ??= GetFieldsByPrefix(rawFields);

    public List<string> GetFieldsOrNull(string name)
    {
        if (!FieldsByName.TryGetValue(name, out var myFields))
            return null;
        if (myFields == null || !myFields.Any() || (myFields.Count == 1 && string.IsNullOrWhiteSpace(myFields.First())))
            return null;
        return myFields;
    }

    internal static Dictionary<string, List<string>> GetFieldsByPrefix(List<string> fields)
    {
        var cleaned = fields?
                          .Select(f => f?.ToLowerInvariant()) //.Trim(Exclusive, Add, Remove))
                          .Where(f => !string.IsNullOrWhiteSpace(f))
                          .ToList()
                      ?? [];

        return cleaned
            .Select(f =>
            {
                // Odata convention https://learn.microsoft.com/en-us/aspnet/web-api/overview/odata-support-in-aspnet-web-api/using-select-expand-and-value#using-select
                var dotIndex = f.IndexOf('/');
                var has2 = dotIndex > -1;
                return new
                {
                    Key = has2 ? f.Substring(0, dotIndex) : Main,
                    Value = has2 ? f.Substring(dotIndex + 1) : f
                };
            })
            .GroupBy(f => f.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => p.Value).ToList(),
                StringComparer.OrdinalIgnoreCase
            );
    }

}