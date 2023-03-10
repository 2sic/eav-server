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
        public const string GlobalEavQueryPrefix = "Eav.Queries.Global.";

        public const string GlobalEavQueryPrefix15 = "Global:";

        /// <summary>
        /// Unsure what this is for, and if there are actually any queries that match this!
        /// </summary>
        public const string GlobalQueryPrefix = "Global.";
    }
}
