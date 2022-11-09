namespace ToSic.Eav.DataSources
{
    public class SqlPlatformInfo
    {
        public const string DefaultConnectionPlaceholder = "(default)";

        public virtual string DefaultConnectionStringName => "unknown-server-please-override-SqlPlatformInfo";

        public virtual string FindConnectionString(string name)
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings[name].ConnectionString;
        }

    }
}
