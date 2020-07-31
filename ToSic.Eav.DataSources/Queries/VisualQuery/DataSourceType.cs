using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources.Queries
{
    /// <summary>
    /// Describes what a DataSource is for in the visual query (for logical grouping)
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
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
        // ReSharper disable once UnusedMember.Global
        Target
    }
}
