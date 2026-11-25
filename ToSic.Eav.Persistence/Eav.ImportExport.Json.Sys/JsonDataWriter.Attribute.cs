using ToSic.Eav.Data.Sys.Attributes;

namespace ToSic.Eav.ImportExport.Json.Sys;

partial class JsonDataWriter
{

    /// <summary>
    /// Create a dictionary with each field, containing another dictionary with the language keys and the values of type T.
    /// </summary>
    /// <typeparam name="T">The underlying value in all these attributes, such as string, int or IEnumerable{IEntity}</typeparam>
    /// <param name="attribs"></param>
    /// <returns></returns>
    private Dictionary<string, Dictionary<string, T?>> ToTypedDictionary<T>(IEnumerable<IAttribute> attribs)
    {
        var l = LogDsDetails.Fn<Dictionary<string, Dictionary<string, T?>>>();
        var result = new Dictionary<string, Dictionary<string, T?>>();
        var attribsTyped = attribs
            .Cast<IAttribute<T>>()
            .ToListOpt();

        foreach (var a in attribsTyped)
        {

            Dictionary<string, T?> dimensions;
            try
            {
                dimensions = a.Typed.ToDictionary(LanguageKeyOfValue, v => v.TypedContents);
            }
            catch (Exception ex)
            {
                string? langList = null;
                try
                {
                    langList = string.Join(",", a.Typed.Select(LanguageKeyOfValue));
                }
                catch
                {
                    /* ignore */
                }

                l.W($"Error building languages list on '{a.Name}', probably multiple identical keys: {langList}");
                l.Done(ex);
                throw;
            }

            try
            {
                result.Add(a.Name, dimensions);
            }
            catch (Exception ex)
            {
                l.W($"Error adding attribute '{a.Name}' to dictionary, probably multiple identical keys");
                l.Done(ex);
                throw;
            }
        }

        return l.ReturnAsOk(result);

        static string LanguageKeyOfValue(IValue v)
        {
            var langs = v.Languages
                .OrderBy(l => l.ReadOnly)
                .Select(l => (l.ReadOnly ? JsonDataConstants.ReadOnlyMarker : "") + l.Key)
                .ToArray();
            return string.Join(",", langs)
                .UseFallbackIfNoValue(JsonDataConstants.NoLanguage);
        }
    }
}
