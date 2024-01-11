using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ToSic.Eav.Data;

namespace ToSic.Eav.ImportExport.Internal.ImportHelpers;

/// <summary>
/// Helpers to handle attribute languages during the import
/// </summary>
internal class AttributeLanguageImportHelper
{

    /// <summary>
    /// Get for example en-US from [ref(en-US,ro)].
    /// </summary>
    public static string GetLanguageInARefTextCode(string valueString)
    {
        var match = Regex.Match(valueString, @"\[ref\((?<language>.+),(?<readOnly>.+)\)\]");
        return match.Success ? match.Groups["language"].Value : null;
    }

    /// <summary>
    /// Get for example ro from [ref(en-US,ro)].
    /// </summary>
    public static string GetValueReferenceProtection(string valueString, string defaultValue = "")
    {
        var match = Regex.Match(valueString, @"\[ref\((?<language>.+),(?<readOnly>.+)\)\]");
        return match.Success ? match.Groups["readOnly"].Value : defaultValue;
    }

    /// <summary>
    /// Get the value of an attribute in the language specified.
    /// </summary>
    public static (IValue Value, IAttribute Attribute) ValueItemOfLanguageOrNull(IDictionary<string, IAttribute> attributes, string key, string language)
    {
        var values = attributes
            .Where(item => item.Key == key)
            .Select(item => item.Value)
            .FirstOrDefault();
        var found = values?.Values.FirstOrDefault(value => value.Languages.Any(dimension => dimension.Key == language));
        return (found, values);
    }

}