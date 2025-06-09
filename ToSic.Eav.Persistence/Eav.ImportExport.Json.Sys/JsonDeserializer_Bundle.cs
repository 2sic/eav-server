using System.Text.Json;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Serialization.Sys.Json;

namespace ToSic.Eav.ImportExport.Json.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class JsonSerializer
{
    public string SerializeJsonBundle(JsonBundle bundleList, int indentation)
    {
        return System.Text.Json.JsonSerializer.Serialize(
            new JsonFormat
            {
                Bundles = [bundleList]
            },
            new JsonSerializerOptions(JsonOptions.UnsafeJsonWithoutEncodingHtml)
            {
                WriteIndented = indentation != 0
            }
        );
    }

    public IList<ContentTypeWithEntities> GetContentTypesFromBundles(JsonFormat package)
    {
        var l = LogDsSummary.Fn<IList<ContentTypeWithEntities>>();
        if (package.Bundles.SafeNone())
            return l.Return([], "none found");

        // Prepare step-by-step for better logs
        var bundlesWithTypes = package.Bundles
            .Where(b => b.ContentTypes.SafeAny())
            .ToListOpt();
        var types = bundlesWithTypes
            .SelectMany(b => b.ContentTypes)
            .ToListOpt();
        l.A($"Bundles: {package.Bundles.Count}; with Types {bundlesWithTypes.Count}; Types: {types.Count}");

        var result = types
            .Select(ConvertContentType)
            .ToListOpt();

        return l.Return(result, $"{result.Count}");
    }

    public IList<IEntity> GetEntitiesFromBundles(JsonFormat package, IEntitiesSource relationshipSource = null)
    {
        var l = LogDsDetails.Fn<IList<IEntity>>();
        if (package.Bundles.SafeNone())
            return l.Return([], "none found");

        // Prepare step-by-step for better logs
        var bundlesWithEntities = package.Bundles
            .Where(b => b.Entities.SafeAny())
            .ToListOpt();
        var jsonEntities = bundlesWithEntities
            .SelectMany(b => b.Entities)
            .ToListOpt();
        l.A($"Bundles: {package.Bundles.Count}; with Entities {bundlesWithEntities.Count}; Entities: {jsonEntities.Count}");

        var result = jsonEntities
            .Select(e => Deserialize(e, true, false, relationshipSource))
            .ToListOpt();

        return l.Return(result, $"{result.Count}");
    }
}