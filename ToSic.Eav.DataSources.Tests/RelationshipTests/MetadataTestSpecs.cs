using ToSic.Eav.Apps;
using ToSic.Eav.DataSources.Caching;

namespace ToSic.Eav.DataSourceTests.RelationshipTests
{
    public class MetadataTestSpecs
    {
        public static IAppIdentity AppIdentity = new AppIdentity(4, 3023);

        // IDs of special tests
        public const string TargetTypeMain = "Main";
        public const int Item2P1H = 21164;
        public const int Item2P1HPrices = 2;
        public const int Item2P1HHelp = 1;
        public const int Item2P1HTotal = Item2P1HPrices + Item2P1HHelp;
        public const int ItemWith1Help = 21165;
        public const int ItemWith1HelpCount = 1;
        public const int ItemsCount = Item2P1HTotal + ItemWith1HelpCount;

        public const string TargetTypeOther = "MainOther";

        public const string HelpTypeName = "MetadataHelp";
        public const int MainHelp = 2;
        public const int TotalHelp = 3;

        public const string PriceTypeName = "MetadataPrice";
        public const int PricesTotal = 2;
        public const int PriceTargetsWithDups = 2;
        public const int PriceTargetsUnique = 1;

        public const int MetaHelpOn1 = 21170;
        public const int MetaHelpOn2 = 21171;
        
    }
}
