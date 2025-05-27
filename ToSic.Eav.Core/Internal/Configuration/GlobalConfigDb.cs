namespace ToSic.Eav.Internal.Configuration;
public static class GlobalConfigDb
{
    /// <summary>
    /// The absolute folder where the beta data is stored, usually ends in "App_Data\system-beta" (or ".databeta")
    /// </summary>
    /// <returns>The folder, can be null if it was never set</returns>
    public static void ConnectionString(this IGlobalConfiguration config, string value)
        => config.SetThis(value);

    /// <summary>
    /// The main folder (absolute) where anything incl. data is stored
    /// </summary>
    /// <returns>The folder, can be null if it was never set</returns>
    public static string ConnectionString(this IGlobalConfiguration config)
        => config.GetThis() ?? throw new ("Couldn't load Connection String as SetConnectionString must have been forgotten");

}
