using ToSic.Sys.Configuration;
using ToSic.Sys.Security.Encryption;

namespace ToSic.Sys.Utils.Tests.Security.Encryption;

/// <summary>
/// Test implementation of IGlobalConfiguration for unit testing
/// </summary>
public class TestGlobalConfiguration : IGlobalConfiguration
{
    private readonly string _cryptoFolder;
    private readonly Dictionary<string, string> _values = new();
    
    public TestGlobalConfiguration(string cryptoFolder)
    {
        _cryptoFolder = cryptoFolder;
        // Set the crypto folder using the extension method pattern
        this.CryptoFolder(cryptoFolder);
    }

    // Remove this direct implementation since we'll use the extension method
    // public string CryptoFolder() => _cryptoFolder;
    // public void CryptoFolder(string value) => _values["CryptoFolder"] = value;

    public string? GetThis(string? key = default)
    {
        _values.TryGetValue(key ?? "", out var value);
        return value;
    }

    public string GetThisErrorOnNull(string? key = default)
    {
        if (!_values.TryGetValue(key ?? "", out var value) || value == null)
            throw new ArgumentNullException(key);
        return value;
    }

    public string? GetThisOrSet(Func<string> generator, string? key = default)
    {
        var keyName = key ?? "";
        if (!_values.ContainsKey(keyName))
            _values[keyName] = generator();
        return _values[keyName];
    }

    public string? SetThis(string? value, string? key = default)
    {
        var keyName = key ?? "";
        var oldValue = _values.TryGetValue(keyName, out var old) ? old : null;
        if (value != null)
            _values[keyName] = value;
        else
            _values.Remove(keyName);
        return oldValue;
    }
}