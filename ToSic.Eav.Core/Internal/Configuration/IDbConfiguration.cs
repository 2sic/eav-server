namespace ToSic.Eav.Internal.Configuration;

/// <summary>
/// Global Eav Configuration - containing the ConnectionString
/// </summary>
/// <remarks>
/// It's populated at startup and statically remembers the connection string.
/// </remarks>
public interface IDbConfiguration
{
    /// <summary>
    /// Db Connection String used in the Eav-Connector
    /// </summary>
    string ConnectionString { get; set; }
}