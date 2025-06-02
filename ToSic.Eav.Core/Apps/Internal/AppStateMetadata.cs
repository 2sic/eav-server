using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;
using ToSic.Lib.Caching.Synchronized;

namespace ToSic.Eav.Apps.State;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppStateMetadata
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

    private static SynchronizedObject<IEntity> BuildSynchedItem(IEntitiesSource parent, string staticName)
    {
        var synched = new SynchronizedObject<IEntity>(parent, () => parent.List.FirstOrDefaultOfType(staticName));
        return synched;
    }


    /// <summary>
    /// The App-Settings or App-Resources
    /// </summary>
    public IEntity MetadataItem => (_appItemSynced ??= BuildSyncedMetadata(Owner, Target.AppType)).Value;
    private SynchronizedObject<IEntity> _appItemSynced;

    public IEntity SystemItem => (_appSystemSynched ??= BuildSynchedItem(Owner, Target.SystemType)).Value;
    private SynchronizedObject<IEntity> _appSystemSynched;

    public IEntity CustomItem => (_appCustomSynced ??= BuildSynchedItem(Owner, Target.CustomType)).Value;
    private SynchronizedObject<IEntity> _appCustomSynced;

}