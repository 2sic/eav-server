using ToSic.Eav.Apps;

namespace ToSic.Eav.Repository.Efc.Tests
{
    public class SpecsTestExportSerialize: IAppIdentity
    {
        public int ZoneId => 4;
        public int AppId => 3013;

        // public int LanguageId = 0; // 37 is also an option...

        public int TestItemToSerialize = 20864;

    }
}
