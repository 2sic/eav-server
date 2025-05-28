namespace ToSic.Eav.Repositories;

/// <summary>
/// Use this to tell the EAV at boot time that there is another folder based repository.
/// This will cause the EAV to load that folders Content-Types and Queries.
/// </summary>
[PublicApi]
public abstract class FolderBasedRepository: RepositoryBase
{
    /// <summary>
    /// Empty constructor is very important, as this is typically used by inheriting classes
    /// </summary>
    protected FolderBasedRepository() : this(true, true) { }

    /// <summary>
    /// This constructor could provide more data, but as of now there is only one possible configuration.
    /// </summary>
    /// <param name="global">Must always be true</param>
    /// <param name="readOnly">Must always be true</param>
    [PrivateApi]
    protected FolderBasedRepository(bool global, bool readOnly)
        : base(global, readOnly, RepositoryTypes.Folder) { }

    public abstract List<string> RootPaths { get; }
}