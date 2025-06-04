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

    public List<ContentTypeWithEntities> GetContentTypesFromBundles(JsonFormat package)
    {
        var l = LogDsSummary.Fn<List<ContentTypeWithEntities>>();
        if (package.Bundles.SafeNone()) return l.Return([], "none found");

        // Prepare step-by-step for better logs
        var bundlesWithTypes = package.Bundles
            .Where(b => b.ContentTypes.SafeAny())
            .ToList();
        var types = bundlesWithTypes
            .SelectMany(b => b.ContentTypes)
            .ToList();
        l.A($"Bundles: {package.Bundles.Count}; with Types {bundlesWithTypes.Count}; Types: {types.Count}");

        var result = types
            .Select(ConvertContentType)
            .ToList();

        return l.Return(result, $"{result.Count}");
    }

    public List<IEntity> GetEntitiesFromBundles(JsonFormat package, IEntitiesSource relationshipSource = null)
    {
        var l = LogDsDetails.Fn<List<IEntity>>();
        if (package.Bundles.SafeNone())
            return l.Return([], "none found");

        // Prepare step-by-step for better logs
        var bundlesWithEntities = package.Bundles
            .Where(b => b.Entities.SafeAny())
            .ToList();
        var jsonEntities = bundlesWithEntities
            .SelectMany(b => b.Entities)
            .ToList();
        l.A($"Bundles: {package.Bundles.Count}; with Entities {bundlesWithEntities.Count}; Entities: {jsonEntities.Count}");

        var result = jsonEntities
            .Select(e => Deserialize(e, true, false, relationshipSource))
            .ToList();

        return l.Return(result, $"{result.Count}");
    }
}