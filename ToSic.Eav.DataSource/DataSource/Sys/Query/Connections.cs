﻿using System.Text.RegularExpressions;

namespace ToSic.Eav.DataSource.Sys.Query;

/// <summary>
/// Helper for DataPipeline Wiring of DataSources
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class Connections
{
    private static readonly Regex WireRegex = new("(?<From>.+):(?<Out>.+)>(?<To>.+):(?<In>.+)", RegexOptions.Compiled);

    /// <summary>
    /// Deserialize a string of Wiring Infos to WireInfo Objects
    /// </summary>
    internal static IList<Connection> Deserialize(string wiringsSerialized)
    {
        if (string.IsNullOrWhiteSpace(wiringsSerialized)) return new List<Connection>();

        var wirings = wiringsSerialized.Split(["\r\n"], StringSplitOptions.None);

        return wirings.Select(wire => WireRegex.Match(wire)).Select(match => new Connection
            {
                From = match.Groups[Connection.FromField].Value,
                Out = match.Groups[Connection.OutField].Value,
                To = match.Groups[Connection.ToField].Value,
                In = match.Groups[Connection.InField].Value
            })
            .ToList();
    }

    /// <summary>
    /// Serialize Wire Infos to a String
    /// </summary>
    internal static string Serialize(IEnumerable<Connection> wirings) => string.Join("\r\n", wirings.Select(w => w.ToString()));
}