using ToSic.Eav.Apps.Sys.State.Managers;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Sys;
using IMetadataSource = ToSic.Eav.Metadata.Sys.IMetadataSource;

namespace ToSic.Eav.Apps.Sys.State;

partial class AppState:
    IHasMetadata,
    IHasMetadataSourceAndExpiring
{
    [PrivateApi]
    internal IMetadata GetMetadataOf<T>(TargetTypes targetType, T key, string title)
        => MetadataManager.GetMetadataOf(targetType: targetType, key: key, npo: default, title: title);
    // till 2025-09-05 was duplicate code:
    // new Metadata<T>((int)targetType, key, title, source: new MetadataProviderApp(MetadataManager));

    [field: AllowNull, MaybeNull]
    private AppMetadataManager MetadataManager => field
        ??= !_loading
            ? throw new("Trying to init metadata, but App is not in loading state.")
            : AppContentTypesFromRepository != null
                ? throw new("Can't init metadata if content-types are already set")
                : new(this, this);

    /// <summary>
    /// Metadata describing this App
    /// </summary>
    /// <remarks>
    /// On first access, various objects are created, which may only be created during loading but before ContentTypes are created.
    /// This is coordinated by the AppStateBuilder.
    /// </remarks>
    [field: AllowNull, MaybeNull]
    public IMetadata Metadata => field ??= GetMetadataOf(TargetTypes.App, AppId, $"App ({AppId}) {Name} ({Folder})");

    /// <inheritdoc />
    public IMetadataSource MetadataSource => MetadataManager;

}