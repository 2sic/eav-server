//using System.Collections.Generic;
//using System.Linq;
//using ToSic.Eav.Data.Builder;
//using ToSic.Eav.Logging;
//using ToSic.Eav.LookUp;
//using ToSic.Eav.TokenEngine.Tests.ValueProvider;

//namespace ToSic.Eav.TokenEngine.Tests.TestData
//{
//    public class DemoConfigs
//    {
//        private const int AppIdX = -1;

//        public static LookUpEngine AppSetAndRes(int appId = AppIdX)
//        {
//            var vc = new LookUpEngine(null as ILog);
//            vc.Add(AppSettings(appId));
//            vc.Add(AppResources(appId));

//            return vc;
//        }

//        public static LookUpInEntity BuildValueProvider(string name, Dictionary<string, object> vals, int appId = AppIdX)
//        {
//            var ent = new Data.Entity(appId, 0, ContentTypeBuilder.Fake(name), vals, vals.FirstOrDefault().Key);
//            return new LookUpInEntity(ent, name);
//        }

//        public static LookUpInEntity AppSettings(int appId)
//        {
//            var vals = new Dictionary<string, object>
//            {
//                {"Title", "App Settings"},
//                {"DefaultCategoryName", ValueCollectionProvider_Test.DefaultCategory},
//                {"MaxPictures", ValueCollectionProvider_Test.MaxPictures},
//                {"PicsPerRow", "3"}
//            };
//            return BuildValueProvider("AppSettings", vals, appId);
//            //var ent = new Data.Entity(appId, 0, "AppSettings", vals, "Title");
//            //return ent;
//        }

//        public static LookUpInEntity AppResources(int appId)
//        {
//            var vals = new Dictionary<string, object>
//            {
//                {"Title", "Resources"},
//                {"Greeting", "Hello there!"},
//                {"Introduction", "Welcome to this"}
//            };
//            return BuildValueProvider("AppResources", vals, appId);
//            //var ent = new Data.Entity(appId, 0, "AppResources", vals, "Title");
//            //return ent;
//        }
//    }
//}
