namespace ToSic.Eav.DataSources
{
    internal class DataSourceConstants
    {
        public const string LogPrefix = "DS";
        public static readonly string RootDataSource = typeof(IAppRoot).AssemblyQualifiedName;

        #region Version Change Constants

        internal const string V3To4DataSourceDllOld = ", ToSic.Eav";
        internal const string V3To4DataSourceDllNew = ", ToSic.Eav.DataSources";

        #endregion

        /// <summary>
        /// Global queries must start with this prefix
        /// </summary>
        public const string SystemQueryPrefixPreV15 = "Eav.Queries.Global.";

        public const string SystemQueryPrefix = "System.";
    }
}
