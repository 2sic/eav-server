using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using static ToSic.Eav.Apps.AppLoadConstants;

namespace ToSic.Eav.Apps
{
    [PrivateApi]
    public partial class AppStateSettings
    {
        
        public IEntity AppSettings
            => (_appSettingsSynched ?? (_appSettingsSynched = BuildSynchedMetadata(TypeAppSettings))).Value;
        private SynchronizedObject<IEntity> _appSettingsSynched;

        public IEntity AppResources
            => (_appResourcesSynched ?? (_appResourcesSynched = BuildSynchedMetadata(TypeAppResources))).Value;
        private SynchronizedObject<IEntity> _appResourcesSynched;
        
        public IEntity AppConfiguration
            => (_appConfigSynched ?? (_appConfigSynched = BuildSynchedMetadata(TypeAppConfig))).Value;
        private SynchronizedObject<IEntity> _appConfigSynched;
        
        private SynchronizedObject<IEntity> BuildSynchedMetadata(string staticName)
        {
            var synched = new SynchronizedObject<IEntity>(Parent,
                () => Parent.AppMetadata.FirstOrDefault(
                    md => md.Type.StaticName == staticName));
            return synched;
        }
    }
}
