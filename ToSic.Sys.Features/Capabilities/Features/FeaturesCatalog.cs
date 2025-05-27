using ToSic.Sys.Startup;

namespace ToSic.Sys.Capabilities.Features;

/// <summary>
/// The features catalog manages all the features which the system registers at startup.
/// </summary>
/// <param name="logStore"></param>
[PrivateApi]
public class FeaturesCatalog(ILogStore logStore) : GlobalCatalogBase<Feature>(logStore, $"Lib.FeatCt", new());