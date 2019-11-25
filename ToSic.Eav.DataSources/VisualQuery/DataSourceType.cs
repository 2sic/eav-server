using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources.VisualQuery
{
    /// <summary>
    /// Describes what a DataSource is for in the visual query (for logical grouping)
    /// </summary>
    [PublicApi]
    public enum DataSourceType
    {
        Cache,
        Filter,
        Logic,
        Lookup,
        Modify,
        Security,
        Sort,
        Source,
        Target
    }
}
