using System.Runtime.CompilerServices;

namespace ToSic.Sys.Configuration;

[PrivateApi("Concrete implementation should remain hidden.")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class GlobalConfiguration : IGlobalConfiguration
{
    internal static IDictionary<string, string> Strings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

    /// <inheritdoc/>>
    public string? GetThis([CallerMemberName] string? key = default)
        => Strings.TryGetValue(key ?? "dummy", out var value)
            ? value
            : null;

    /// <inheritdoc/>>
    public string GetThisOrSet(Func<string> generator, [CallerMemberName] string? key = default)
    {
        var value = GetThis(key);
        if (value != null)
            return value;
        value = generator();
        SetThis(value, key);
        return value;
    }

    /// <inheritdoc/>>
    public string GetThisErrorOnNull([CallerMemberName] string? key = default)
        => GetThis(key!)
           ?? throw new ArgumentNullException(ErrorMessageNullNotAllowed(key!));

    /// <inheritdoc/>>
    [return: NotNullIfNotNull(nameof(value))]
    public string? SetThis(string? value, [CallerMemberName] string? key = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key), @"Key cannot be null or empty.");
        Strings[key!] = value!;
        return value;
    }

    
    public static string ErrorMessageNullNotAllowed(string fieldName) =>
        $"{nameof(IGlobalConfiguration)}.{fieldName} cannot be null. " +
        $"Make sure it's set upon initial creation of the dependencies etc.";

}