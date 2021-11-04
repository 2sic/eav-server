using ToSic.Eav.Apps;

namespace ToSic.Eav.Repository.Efc.Tests
{
    public class TestValuesOnPc2Dm: IAppIdentity
    {
        public int RootAppId = 1;
        public int AppId => 2;
        public int ZoneId => 2;

        //public IAppIdentity TestApp => new AppIdentity(ZoneId, AppId);

        //public int LanguageId = 0; // 37 is also an option...

        public int BlogAppId = 78;

        public int ItemOnHomeId = 3269;
        public int ContentBlockItemWith9Items = 4645;
    }
}
