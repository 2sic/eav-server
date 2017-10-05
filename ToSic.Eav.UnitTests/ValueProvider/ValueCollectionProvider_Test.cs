using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.UnitTests.ValueProvider
{
    [TestClass]
    public class ValueCollectionProvider_Test
    {
        private const int AppIdX = -1;

        #region Important Values which will be checked when resolved
        public string OriginalSettingDefaultCat = "[AppSettings:DefaultCategoryName]";
        private readonly string ResolvedSettingDefaultCat = "All";
        private readonly string OriginalSettingMaxItems = "[AppSettings:MaxItems||100]";
        public static string ResolvedSettingMaxItems = "100";
        public static string MaxPictures = "21";
        public static string DefaultCategory = "All";
        #endregion

        [TestMethod]
        public void ValueCollection_GeneralFunctionality()
        {
            var vc = ValueCollection();
            var Settings = this.Settings();
            Assert.IsTrue(vc.Sources.Count == 2, "Should have 2 sources");
            Assert.AreEqual("App Settings", vc.Sources["appsettings"].Get("Title") );
            Assert.AreEqual(OriginalSettingDefaultCat, Settings["DefaultCategory"]);
            Assert.AreEqual(OriginalSettingMaxItems, Settings["MaxItems"]);

            vc.LoadConfiguration(Settings);

            Assert.AreEqual(ResolvedSettingDefaultCat, Settings["DefaultCategory"], "Default should be all");
            Assert.AreEqual(ResolvedSettingMaxItems, Settings["MaxItems"], "Max should be 100");
        }

        public IValueCollectionProvider ValueCollection()
        {
            var vc = new ValueCollectionProvider();
            var entVc = new EntityValueProvider(AppSettings(), "AppSettings");
            vc.Sources.Add("AppSettings".ToLower(), entVc);
            vc.Sources.Add("AppResources".ToLower(), new EntityValueProvider(AppResources(), "AppResources"));

            return vc;
        }


        public Dictionary<string, string> Settings()
        {
            return new Dictionary<string, string>()
            {
                {"Title", "Settings"},
                {"DefaultCategory", OriginalSettingDefaultCat},
                {"MaxItems", OriginalSettingMaxItems},
                {"PicsPerRow", "3"}
            };
        }

        public Dictionary<string, string> ResolvedSettings()
        {
            var settings = Settings();
            ValueCollection().LoadConfiguration(settings);
            return settings;
        }

        public ToSic.Eav.Interfaces.IEntity AppSettings()
        {
            var vals = new Dictionary<string, object>()
            {
                {"Title", "App Settings"},
                {"DefaultCategoryName", DefaultCategory},
                {"MaxPictures", MaxPictures},
                {"PicsPerRow", "3"}
            };

            var ent = new ToSic.Eav.Data.Entity(AppIdX, 305200, "AppSettings", vals, "Title");
            return ent;
        }

        public ToSic.Eav.Interfaces.IEntity AppResources()
        {
            var vals = new Dictionary<string, object>()
            {
                {"Title", "Resources"},
                {"Greeting", "Hello there!"},
                {"Introduction", "Welcome to this"}
            };
            var ent = new Eav.Data.Entity(AppIdX, 305200, "AppResources", vals, "Title");
            return ent;
        }
    }
}
