using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Stack;
using ToSic.Eav.Data.Entities.Sys.Lists;
using ToSic.Eav.Data.Entities.Sys.Sources;
using ToSic.Sys.Caching.Synchronized;

namespace ToSic.Eav.Apps.State;

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


    public IEntity AppConfiguration => (_appConfigSynced ??= BuildSyncedMetadata(Owner, AppLoadConstants.TypeAppConfig)).Value;
    private SynchronizedObject<IEntity> _appConfigSynced;

    private static SynchronizedObject<IEntity> BuildSyncedMetadata(IAppStateCache parent, string staticName)
    {
        var synced = new SynchronizedObject<IEntity>(parent, () => parent.Metadata.FirstOrDefaultOfType(staticName));
        return synced;
    }

    private static SynchronizedObject<IEntity> BuildSyncedItem(IEntitiesSource parent, string staticName)
    {
        var synced = new SynchronizedObject<IEntity>(parent, () => parent.List.FirstOrDefaultOfType(staticName));
        return synced;
    }


    /// <summary>
    /// The App-Settings or App-Resources
    /// </summary>
    public IEntity MetadataItem => (_appItemSynced ??= BuildSyncedMetadata(Owner, Target.AppType)).Value;
    private SynchronizedObject<IEntity> _appItemSynced;

    public IEntity SystemItem => (_appSystemSynched ??= BuildSyncedItem(Owner, Target.SystemType)).Value;
    private SynchronizedObject<IEntity> _appSystemSynched;

    public IEntity CustomItem => (_appCustomSynced ??= BuildSyncedItem(Owner, Target.CustomType)).Value;
    private SynchronizedObject<IEntity> _appCustomSynced;

}