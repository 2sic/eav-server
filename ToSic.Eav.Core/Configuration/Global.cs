using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi]
    public class Global
    {
        public const string GroupQuery = "query";
        public const string GroupConfiguration = "configuration";

        private const int MagicZeroMaker = 10000000;
        public const int GlobalContentTypeMin = int.MaxValue / MagicZeroMaker * MagicZeroMaker;
        public const int GlobalContentTypeSourceSkip = 1000;

        public const int GlobalEntityIdMin = int.MaxValue / MagicZeroMaker * MagicZeroMaker;
        public const int GlobalEntitySourceSkip = 10000;
    }
}
