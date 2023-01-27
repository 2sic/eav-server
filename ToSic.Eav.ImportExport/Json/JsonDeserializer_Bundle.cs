using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Serialization;
using ToSic.Lib.Logging;


// ReSharper disable once CheckNamespace
namespace ToSic.Eav.ImportExport.Json
{
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

        internal List<IContentType> GetContentTypesFromBundles(JsonFormat package) => Log.Func(l =>
        {
            if (package.Bundles?.Any() != true) return (new List<IContentType>(), "none found");

            // Prepare step-by-step for better logs
            var bundlesWithTypes = package.Bundles
                .Where(b => b.ContentTypes?.Any() == true)
                .ToList();
            var types = bundlesWithTypes
                .SelectMany(b => b.ContentTypes)
                .ToList();
            l.A($"Bundles: {package.Bundles.Count}; with Types {bundlesWithTypes.Count}; Types: {types.Count}");

            var result = types
                .Select(ConvertContentType)
                .ToList();

            return (result, $"{result.Count}");
        });

        internal List<IEntity> GetEntitiesFromBundles(JsonFormat package, IEntitiesSource relationshipSource = null) => Log.Func(l =>
        {
            if (package.Bundles?.Any() != true) return (new List<IEntity>(), "none found");

            // Prepare step-by-step for better logs
            var bundlesWithEntities = package.Bundles
                .Where(b => b.Entities?.Any() == true)
                .ToList();
            var entities = bundlesWithEntities
                .SelectMany(b => b.Entities)
                .ToList();
            l.A($"Bundles: {package.Bundles.Count}; with Entities {bundlesWithEntities.Count}; Entities: {entities.Count}");

            var result = entities
                .Select(e => Deserialize(e, true, false, relationshipSource))
                .ToList();

            return (result, $"{result.Count}");
        });
    }
}
