using ToSic.Eav.Caching;
using ToSic.Eav.Data;

namespace ToSic.Eav.Apps
{
    public class AppStateMetadata
    {
        /// <summary>
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="target"></param>
        internal AppStateMetadata(AppState owner, AppThingsIdentifiers target)
        {
            Owner = owner;
            Target = target;
        }

        private AppState Owner { get; }
        private AppThingsIdentifiers Target { get; }


        public IEntity AppConfiguration
            => (_appConfigSynched ?? (_appConfigSynched = BuildSynchedMetadata(Owner, AppLoadConstants.TypeAppConfig))).Value;
        private SynchronizedObject<IEntity> _appConfigSynched;

        private static SynchronizedObject<IEntity> BuildSynchedMetadata(AppState parent, string staticName)
        {
            var synched = new SynchronizedObject<IEntity>(parent,
                () => parent.Metadata.FirstOrDefaultOfType(staticName));
            return synched;
        }

        private static SynchronizedObject<IEntity> BuildSynchedItem(AppState parent, string staticName)
        {
            var synched = new SynchronizedObject<IEntity>(parent,
                () => parent.List.FirstOrDefaultOfType(staticName));
            return synched;
        }


        /// <summary>
        /// The App-Settings or App-Resources
        /// </summary>
        public IEntity MetadataItem => (_appItemSynced ?? (_appItemSynced = BuildSynchedMetadata(Owner, Target.AppType))).Value;
        private SynchronizedObject<IEntity> _appItemSynced;

        public IEntity SystemItem => (_appSystemSynched ?? (_appSystemSynched = BuildSynchedItem(Owner, Target.SystemType))).Value;
        private SynchronizedObject<IEntity> _appSystemSynched;

        public IEntity CustomItem => (_appCustomSynced ?? (_appCustomSynced = BuildSynchedItem(Owner, Target.CustomType))).Value;
        private SynchronizedObject<IEntity> _appCustomSynced;

    }
}
