namespace ToSic.Eav.DataSources
{
    public abstract partial class DataSource
    {
        /// <summary>
        /// Correct prefix to use when retrieving a value from the current data sources configuration entity.
        /// </summary>
        public static string MyConfig = "Settings"; // WIP

        private static string MyConfigOld = "Settings";

        /// <inheritdoc />
        public IDataSourceConfiguration Configuration => _config ?? (_config = new DataSourceConfiguration(Deps.ConfigDependencies, this));
        private IDataSourceConfiguration _config;

    }
}