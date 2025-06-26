using ToSic.Sys.Configuration;

namespace ToSic.Sys.Security.Encryption;

public static class GlobalConfigCryptoExtensions
{
    /// <summary>
    /// The absolute secure folder where generated RSA keys are stored.
    /// </summary>
    public static string CryptoFolder(this IGlobalConfiguration config)
        => config.GetThisErrorOnNull();

    /// <summary>
    /// The absolute secure folder where generated RSA keys are stored.
    /// </summary>
    public static void CryptoFolder(this IGlobalConfiguration config, string value)
        => config.SetThis(value);
}
