namespace ToSic.Eav.DataSource;

/// <summary>
/// This interface marks objects which can provide links to DataSources.
/// In most cases, the link references the data source itself.
///
/// The returned link will point to one or more DataSources, and can be extended to contain more links.
/// This is important to connect DataSources together.
/// </summary>
/// <remarks>
/// The name may be a bit misleading, it could also be `IHasDataSourceLink`.
/// But because it's visible a lot in public APIs where one or many links could be provided,
/// we believe this is the best name for this use case.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface IDataSourceLinkable
{
    /// <summary>
    /// A link - or possibly many.
    /// In most cases, this references the parent object which provides this/these links.
    /// </summary>
    IDataSourceLink Link { get; }
}