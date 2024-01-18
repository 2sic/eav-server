namespace ToSic.Eav.DataSources.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class SqlPlatformInfo
{
    public const string DefaultConnectionPlaceholder = "(default)";

    public virtual string DefaultConnectionStringName => "unknown-server-please-override-SqlPlatformInfo";

    public virtual string FindConnectionString(string name)
    {
        return System.Configuration.ConfigurationManager.ConnectionStrings[name].ConnectionString;
    }

}