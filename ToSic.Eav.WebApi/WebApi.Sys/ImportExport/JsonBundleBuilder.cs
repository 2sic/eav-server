using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.Persistence.File;

namespace ToSic.Eav.WebApi.Sys.ImportExport;
internal class JsonBundleBuilder(IAppReader appReader, ILog parentLog) : HelperBase(parentLog, "Eav.BDHlp")
{
    internal JsonBundle BundleBuild(ExportConfiguration export, JsonSerializer serializer)
    {
        var l = Log.Fn<JsonBundle>($"build bundle for ExportConfiguration:{export.Guid}");

        // loop through content types and add them to the bundle-list
        l.A($"count export content types:{export.ContentTypes.Count}");
        var serSettings = new JsonSerializationSettings
        {
            CtIncludeInherited = true,
            CtAttributeIncludeInheritedMetadata = false
        };

        // Content-Types contains the Content-Type as well entities referenced in CT-Attribute Metadata such as Formulas
        var bundleTypesRaw = export.ContentTypes
                .Select(appReader.GetContentType)
                .Select(ct => serializer.ToPackage(ct, serSettings))
                .Select(jsonType => new JsonContentTypeSet
                {
                    ContentType = PreserveMarker(export.PreserveMarkers,
                        jsonType.ContentType
                        ?? throw new("Error accessing ContentType in bundle which must have it.")
                    ),
                    Entities = jsonType.Entities
                })
                .ToList();

        // loop through entities and add them to the bundle list
        l.A($"count export entities:{export.Entities.Count}");

        var mdDepth = export.EntitiesWithMetadata
            ? FileSystemLoaderConstants.QueryMetadataDepth
            : 0;
        IList<JsonEntity> bundleEntitiesRaw = export.Entities
                .Select(e => appReader.List.GetOne(e))
                .Select(e => serializer.ToJson(e, mdDepth)!)
                .ToList(); // must be mutable ToList!

        var bundleList = new BundleEntityDeduplicateHelper(Log).CleanBundle(new()
        {
            ContentTypes = bundleTypesRaw,
            Entities = bundleEntitiesRaw
        });

        return l.ReturnAsOk(bundleList);
    }

    private JsonContentType PreserveMarker(bool preserveMarkers, JsonContentType jsonContentType)
    {
        var l = Log.Fn<JsonContentType>($"preserveMarkers:{preserveMarkers}");
        if (preserveMarkers)
            return jsonContentType;

        if (jsonContentType.Metadata == null)
            return jsonContentType;

        var removeQue = jsonContentType.Metadata
            .Where(metaData => metaData.Type.Name == ExportDecorator.ContentTypeName)
            .ToList();

        var trimmedMetadata = jsonContentType.Metadata
            .Where(md => !removeQue.Contains(md))
            .ToListOpt();

        jsonContentType = jsonContentType with { Metadata = trimmedMetadata };

        //foreach (var item in removeQue)
        //    jsonContentType.Metadata.Remove(item);

        return l.Return(jsonContentType);
    }

}
