using ToSic.Eav.Apps.Sys.Stack;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Sys.Caching.Synchronized;

namespace ToSic.Eav.Apps.Sys.State;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppStateMetadata : IAppStateMetadata
{
    /// <summary>
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="target"></param>
    internal AppStateMetadata(IAppStateCache owner, AppThingsIdentifiers target)
    {
        Owner = owner;
        Target = target;
    }

    private IAppStateCache Owner { get; }
    private AppThingsIdentifiers Target { get; }


    public IEntity? AppConfiguration => (_appConfigSynced ??= BuildSyncedItem(Owner, AppLoadConstants.TypeAppConfig, true)).Value;
    private SynchronizedObject<IEntity?>? _appConfigSynced;

    private static SynchronizedObject<IEntity?> BuildSyncedItem(IAppStateCache parent, string staticName, bool useMetadata)
        => new(parent, () => (useMetadata ? parent.Metadata : parent.List)
            .FirstOrDefaultOfType(staticName)
        );


    /// <summary>
    /// The App-Settings or App-Resources
    /// </summary>
    public IEntity? MetadataItem => (_appItemSynced ??= BuildSyncedItem(Owner, Target.AppType, true)).Value;
    private SynchronizedObject<IEntity?>? _appItemSynced;

    public IEntity? SystemItem => (_appSystemSynced ??= BuildSyncedItem(Owner, Target.SystemType, false)).Value;
    private SynchronizedObject<IEntity?>? _appSystemSynced;

    public IEntity? CustomItem => (_appCustomSynced ??= BuildSyncedItem(Owner, Target.CustomType, false)).Value;
    private SynchronizedObject<IEntity?>? _appCustomSynced;

}