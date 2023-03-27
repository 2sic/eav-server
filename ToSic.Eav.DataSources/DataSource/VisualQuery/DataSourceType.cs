using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSource.VisualQuery
{
    /// <summary>
    /// Describes what a DataSource is for in the visual query (for logical grouping)
    /// </summary>
    [PublicApi]
    public enum DataSourceType
    {
        /// <summary>
        /// DataSources of Type App only exist on the app.
        /// </summary>
        App,

        Cache,
        Filter,
        Logic,
        Lookup,
        Modify,
        Security,
        Sort,
        Source,
        Target,
        Debug,
        System
    }
}
