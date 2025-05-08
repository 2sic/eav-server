using System.Collections.Generic;
using System.Runtime.CompilerServices;

//using static ToSic.Eav.Internal.Configuration.GlobalConfigHelpers;

namespace ToSic.Eav.Internal.Configuration;

public class GlobalConfiguration : IGlobalConfiguration
{
    internal static IDictionary<string, string> Strings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

    public string? GetThis([CallerMemberName] string key = default)
        => Strings.TryGetValue(key ?? "dummy", out var value)
            ? value
            : null;

    public string GetThisOrSet(Func<string> generator, [CallerMemberName] string key = default)
    {
        var value = GetThis(key);
        if (value != null)
            return value;
        value = generator();
        SetThis(value, key);
        return value;
    }

    public string? GetThisErrorOnNull([CallerMemberName] string key = default)
        => GetThis(key) ?? throw new ArgumentNullException(ErrorMessageNullNotAllowed(nameof(key)));

    public string SetThis(string value, [CallerMemberName] string key = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key), @"Key cannot be null or empty.");
        Strings[key] = value;
        return value;
    }

    // TODO: duplicate
    public static string ErrorMessageNullNotAllowed(string fieldName) =>
        $"ISystemFoldersConfiguration.{fieldName} cannot be null. Make sure it's set upon initial creation of the dependencies etc.";

}