namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonDataWriter
{
    private Dictionary<string, Dictionary<string, string?>> ConvertReferences(Dictionary<string, Dictionary<string, string?>> links, Guid entityGuid)
    {
        var l = LogDsDetails.Fn<Dictionary<string, Dictionary<string, string?>>>();
        try
        {
            var converter = valConverter.Value;
            var converted = links.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToDictionary(
                    val => val.Key,
                    val => converter.ToValue(val.Value, entityGuid)
                )
            );
            return l.ReturnAsOk(converted);
        }
        catch (Exception ex)
        {
            l.A("Ran into an error. Will log bug ignore and return original");
            l.Ex(ex);
            return l.Return(links, "error/ignored");
        }
    }

}
