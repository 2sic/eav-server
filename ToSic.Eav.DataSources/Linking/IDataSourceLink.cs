namespace ToSic.Eav.DataSources.Linking
{
    /// <summary>
    /// This interface marks objects which can provide links to DataSources.
    /// In most cases, the link references the data source itself.
    ///
    /// The returned link will point to one or more DataSources, and can be extended to contain more links.
    /// This is important to connect DataSources together.
    /// </summary>
    public interface IDataSourceLink
    {
        IDataSourceLinkInfo Link { get; }
    }
}
