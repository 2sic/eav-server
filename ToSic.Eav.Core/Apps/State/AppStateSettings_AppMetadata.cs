//using System.Linq;
//using ToSic.Eav.Caching;
//using ToSic.Eav.Data;
//using ToSic.Eav.Documentation;
//using static ToSic.Eav.Apps.AppLoadConstants;

//namespace ToSic.Eav.Apps
//{
//    [PrivateApi]
//    public partial class AppSettingsStack
//    {

//        public IEntity AppConfiguration
//            => (_appConfigSynched ?? (_appConfigSynched = BuildSynchedMetadata(Owner, TypeAppConfig))).Value;
//        private SynchronizedObject<IEntity> _appConfigSynched;

//        internal static SynchronizedObject<IEntity> BuildSynchedMetadata(AppState parent, string staticName)
//        {
//            var synched = new SynchronizedObject<IEntity>(parent,
//                () => parent.Metadata.FirstOrDefault(md => md.Type.NameId == staticName));
//            return synched;
//        }
//    }
//}
