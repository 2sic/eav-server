using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.Core.Tests.LookUp
{
    public class LookUpTestData
    {
        public const string KeyAppSettings = "AppSettings";
        public const string KeyAppResources = "AppResources";

        private const int AppIdX = -1;

        public static LookUpEngine EmptyLookupEngine => new LookUpEngine(null);

        public static LookUpEngine AppSetAndRes(int appId = AppIdX)
        {
            var vc = EmptyLookupEngine;
            vc.Add(AppSettings(appId));
            vc.Add(AppResources(appId));

            return vc;
        }

        public static LookUpInEntity BuildLookUpEntity(string name, Dictionary<string, object> values, int appId = AppIdX)
        {
            var ent = new Eav.Data.Entity(appId, 0, new ContentTypeBuilder().Transient(name), values, values.FirstOrDefault().Key);
            return new LookUpInEntity(name, ent, null);
        }

        public static LookUpInEntity AppSettings(int appId)
        {
            var values = new Dictionary<string, object>
            {
                {"Title", "App Settings"},
                {"DefaultCategoryName", LookUpEngineTests.DefaultCategory},
                {"MaxPictures", LookUpEngineTests.MaxPictures},
                {"PicsPerRow", "3"}
            };
            return BuildLookUpEntity(KeyAppSettings, values, appId);
        }

        public static LookUpInEntity AppResources(int appId)
        {
            var values = new Dictionary<string, object>
            {
                {"Title", "Resources"},
                {"Greeting", "Hello there!"},
                {"Introduction", "Welcome to this"}
            };
            return BuildLookUpEntity(KeyAppResources, values, appId);
        }
    }
}
