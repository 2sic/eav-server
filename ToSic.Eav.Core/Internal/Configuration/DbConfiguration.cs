namespace ToSic.Eav.Internal.Configuration;

/// <summary>
/// Global Eav Configuration
/// </summary>
public class DbConfiguration : IDbConfiguration
{
    /// <summary>
    /// The underlying value is static, so it only has to be set at startup.
    /// </summary>
    private static string _conStr;

    #region Internal delivery for depedency injection...

    /// <inheritdoc />
    public string ConnectionString
    {
        get => _conStr 
               ?? throw new("Couldn't load Connection String as SetConnectionString must have been forgotten");
        // Make sure the connection string is only set once
        // And never updated
        // Important to avoid problems and have changing system fingerprints during runtime
        // If you plan on changing this, make sure you discuss w/2dm as it can have far, complex consequences
        set => _conStr ??= value;
    }

    #endregion

}