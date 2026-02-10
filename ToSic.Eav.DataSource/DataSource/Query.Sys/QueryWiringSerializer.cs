using System.Text.RegularExpressions;

namespace ToSic.Eav.DataSource.Sys.Query;

/// <summary>
/// Helper for DataPipeline Wiring of DataSources
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class QueryWiringSerializer
{
    private static readonly Regex WireRegex = new("(?<From>.+):(?<Out>.+)>(?<To>.+):(?<In>.+)", RegexOptions.Compiled);

    /// <summary>
    /// Deserialize a string of Wiring Infos to WireInfo Objects
    /// </summary>
    internal static IList<QueryWire> Deserialize(string wiringsSerialized)
    {
        if (string.IsNullOrWhiteSpace(wiringsSerialized))
            return new List<QueryWire>();

        var wirings = wiringsSerialized.Split(["\r\n"], StringSplitOptions.None);

        return wirings
            .Select(wire => WireRegex.Match(wire))
            .Select(match => new QueryWire
            {
                From = match.Groups[QueryWire.FromField].Value,
                Out = match.Groups[QueryWire.OutField].Value,
                To = match.Groups[QueryWire.ToField].Value,
                In = match.Groups[QueryWire.InField].Value
            })
            .ToList();
    }

    /// <summary>
    /// Serialize Wire Infos to a String
    /// </summary>
    internal static string Serialize(IEnumerable<QueryWire> wirings) =>
        string.Join("\r\n", wirings.Select(w => w.ToString()));
}