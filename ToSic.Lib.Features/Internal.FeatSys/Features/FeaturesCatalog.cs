using ToSic.Eav.Internal.Catalogs;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

[PrivateApi]
public class FeaturesCatalog(ILogStore logStore) : GlobalCatalogBase<Feature>(logStore, $"Lib.FeatCt", new());