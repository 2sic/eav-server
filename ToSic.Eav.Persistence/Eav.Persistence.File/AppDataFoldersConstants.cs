namespace ToSic.Eav.Internal.Loaders;

[PrivateApi]
public class AppDataFoldersConstants
{
    public const string TypesFolder = "contenttypes";
    public const string QueriesFolder = "queries";
    public const string ConfigFolder = "configurations";
    public const string EntitiesFolder = "entities";
    public const string BundlesFolder = "bundles";

    public static string[] EntityItemFolders = [BundlesFolder, QueriesFolder, EntitiesFolder];

}