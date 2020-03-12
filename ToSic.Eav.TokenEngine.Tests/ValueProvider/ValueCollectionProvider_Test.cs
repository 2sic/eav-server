//using System.Collections.Generic;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using ToSic.Eav.TokenEngine.Tests.TestData;

//namespace ToSic.Eav.TokenEngine.Tests.ValueProvider
//{
//    [TestClass]
//    public class ValueCollectionProvider_Test
//    {

//        #region Important Values which will be checked when resolved
//        public string OriginalSettingDefaultCat = "[AppSettings:DefaultCategoryName]";
//        private readonly string ResolvedSettingDefaultCat = "All";
//        private readonly string OriginalSettingMaxItems = "[AppSettings:MaxItems||100]";
//        public static string ResolvedSettingMaxItems = "100";
//        public static string MaxPictures = "21";
//        public static string DefaultCategory = "All";
//        #endregion

//        [TestMethod]
//        public void ValueCollection_GeneralFunctionality()
//        {
//            var vc = DemoConfigs.AppSetAndRes();
//            var settings = Settings();
//            Assert.IsTrue(vc.Sources.Count == 2, "Should have 2 sources");
//            Assert.AreEqual("App Settings", vc.Sources["appsettings"].Get("Title") );
//            Assert.AreEqual(OriginalSettingDefaultCat, settings["DefaultCategory"]);
//            Assert.AreEqual(OriginalSettingMaxItems, settings["MaxItems"]);

//            settings = vc.LookUp(settings);

//            Assert.AreEqual(ResolvedSettingDefaultCat, settings["DefaultCategory"], "Default should be all");
//            Assert.AreEqual(ResolvedSettingMaxItems, settings["MaxItems"], "Max should be 100");
//        }




//        public IDictionary<string, string> Settings()
//        {
//            return new Dictionary<string, string>()
//            {
//                {"Title", "Settings"},
//                {"DefaultCategory", OriginalSettingDefaultCat},
//                {"MaxItems", OriginalSettingMaxItems},
//                {"PicsPerRow", "3"}
//            };
//        }

//        public IDictionary<string, string> ResolvedSettings()
//        {
//            var settings = Settings();
//            DemoConfigs.AppSetAndRes().LookUp(settings);
//            return settings;
//        }
//    }
//}
