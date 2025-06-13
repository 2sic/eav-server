using ToSic.Eav.Apps.Sys.State.Managers;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Sys;

namespace ToSic.Eav.Apps.Sys.State;

partial class AppState:
    IHasMetadata,
    IHasMetadataSourceAndExpiring
{
    [PrivateApi]
    internal IMetadataOf GetMetadataOf<T>(TargetTypes targetType, T key, string title)
        => new MetadataOf<T>((int)targetType, key, title, appSource: MetadataManager);

    [field: AllowNull, MaybeNull]
    private AppMetadataManager MetadataManager => field ??= CreateMetadataManager();

    private AppMetadataManager CreateMetadataManager()
    {
        if (!Loading)
            throw new("Trying to init metadata, but App is not in loading state.");
        if (AppContentTypesFromRepository != null)
            throw new("Can't init metadata if content-types are already set");

        return new(this, this);
    }

    /// <summary>
    /// Metadata describing this App
    /// </summary>
    /// <remarks>
    /// On first access, various objects are created, which may only be created during loading but before ContentTypes are created.
    /// This is coordinated by the AppStateBuilder.
    /// </remarks>
    [field: AllowNull, MaybeNull]
    public IMetadataOf Metadata => field ??= GetMetadataOf(TargetTypes.App, AppId, $"App ({AppId}) {Name} ({Folder})");

    /// <inheritdoc />
    public IMetadataSource MetadataSource => MetadataManager;

}