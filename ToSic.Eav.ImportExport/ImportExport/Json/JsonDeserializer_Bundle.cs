using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Serialization;
using ToSic.Lib.Logging;


// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class JsonSerializer
{
    public string SerializeJsonBundle(JsonBundle bundleList, int indentation) =>
        System.Text.Json.JsonSerializer.Serialize(new JsonFormat
        {
            Bundles = new List<JsonBundle> { bundleList }
        }, new JsonSerializerOptions(JsonOptions.UnsafeJsonWithoutEncodingHtml)
        {
            WriteIndented = indentation != 0
        });

    public List<IContentType> GetContentTypesFromBundles(JsonFormat package)
    {
        var l = Log.Fn<List<IContentType>>();
        if (package.Bundles.SafeNone()) return l.Return(new List<IContentType>(), "none found");

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
        var l = Log.Fn<List<IEntity>>();
        if (package.Bundles.SafeNone()) return l.Return(new List<IEntity>(), "none found");

        // Prepare step-by-step for better logs
        var bundlesWithEntities = package.Bundles
            .Where(b => b.Entities.SafeAny())
            .ToList();
        var entities = bundlesWithEntities
            .SelectMany(b => b.Entities)
            .ToList();
        l.A($"Bundles: {package.Bundles.Count}; with Entities {bundlesWithEntities.Count}; Entities: {entities.Count}");

        var result = entities
            .Select(e => Deserialize(e, true, false, relationshipSource))
            .ToList();

        return l.Return(result, $"{result.Count}");
    }
}