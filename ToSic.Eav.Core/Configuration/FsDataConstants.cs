using ToSic.Lib.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi]
    public class FsDataConstants
    {
        public const string TypesFolder = "contenttypes";
        public const string QueriesFolder = "queries";
        public const string ConfigFolder = "configurations";
        public const string EntitiesFolder = "entities";
        public static string[] EntityItemFolders = { QueriesFolder, EntitiesFolder };

        /// <summary>
        /// Special constant to make our global numbers have a lot of trailing zeros
        /// </summary>
        private const int MagicZeroMaker = 10000000;
        public const int GlobalContentTypeMin = int.MaxValue / MagicZeroMaker * MagicZeroMaker;
        public const int GlobalContentTypeSourceSkip = 1000;

        public const int GlobalEntityIdMin = int.MaxValue / MagicZeroMaker * MagicZeroMaker;
        public const int GlobalEntitySourceSkip = 10000;
    }
}
