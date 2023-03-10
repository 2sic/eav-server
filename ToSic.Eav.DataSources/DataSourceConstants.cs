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

        public const string GlobalEavQueryPrefix15 = "System:";

        /// <summary>
        /// Unsure what this is for, and if there are actually any queries that match this!
        ///
        /// it appears to not be part of the name (seems to get removed) but a key to look in parents - probably drop
        /// </summary>
        public const string GlobalQueryPrefix = "Global.";
    }
}
