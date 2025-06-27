using ToSic.Eav.ImportExport.Json.V1;

namespace ToSic.Eav.WebApi.Sys.ImportExport;
internal class BundleEntityDeduplicateHelper(ILog parentLog) : HelperBase(parentLog, "Eav.BDHlp")
{

    internal JsonBundle CleanBundle(JsonBundle withDuplicates)
    {
        var l = Log.Fn<JsonBundle>($"deduplicate entities in bundle, count types:{withDuplicates.ContentTypes?.Count}, count entities:{withDuplicates.Entities?.Count}");
        var bundleTypesRaw = withDuplicates.ContentTypes ?? [];
        var bundleEntitiesRaw = withDuplicates.Entities ?? [];
        // Find duplicate related entities
        // as there are various ways they can appear, but we really only need them once
        var ctEntities = bundleTypesRaw
            .SelectMany(ct => (ct.Entities ?? [])
                .Select(e => new
                {
                    OwnerCtSet = (JsonContentTypeSet?)ct,
                    OwnerEntity = (JsonEntity?)null,
                    Entity = e,
                    Priority = 1,
                    //List = ct.Entities!
                }))
            .ToListOpt();

        var bundleEntities = bundleEntitiesRaw
            .Select(e => new
            {
                OwnerCtSet = (JsonContentTypeSet?)null,
                OwnerEntity = (JsonEntity?)e,
                Entity = e,
                Priority = 0,
                //List = bundleEntitiesRaw!
            })
            .ToListOpt();

        var dupEntities = ctEntities
            .Concat(bundleEntities)
            .GroupBy(e => e.Entity.Id)
            .Where(g => g.Count() > 1)
            .ToListOpt();

        l.A($"Found {dupEntities.Count} entities-groups (by duplicates) in export.");

        var removes = dupEntities
            .SelectMany(dupEntity =>
            {
                // To pick keepers we prefer the ones on the content-type,
                // but otherwise (assuming duplicates) we just keep the first
                var keep = dupEntity
                               .FirstOrDefault(e => e.Priority == 1)
                           ?? dupEntity.First();
                return dupEntity
                    .Where(e => e != keep)
                    .ToList();
            })
            .ToList();

        // Remove the entities from the types bundles
        var typesFinal = bundleTypesRaw
            .Select(t =>
            {
                var typeRemovals = removes
                    .Where(r => r.OwnerCtSet == t)
                    .ToListOpt();
                if (typeRemovals.Count == 0)
                    return t;
                // remove the entities from the content-type
                return t with
                {
                    Entities = t.Entities! // Entities must exist here, since otherwise the removal-list wouldn't mention it
                        .Where(e => typeRemovals.All(r => r.Entity.Id != e.Id))
                        .ToListOpt()
                };
            })
            .ToList();

        // Remove the entities from the entities bundle
        var entityRemovals = removes
            .Where(r => r.OwnerEntity != null)
            .ToListOpt();

        var entitiesFinal = bundleEntitiesRaw
            .Where(e => entityRemovals.All(er => er.Entity.Id != e.Id))
            .ToListOpt();

        return new()
        {
            ContentTypes = typesFinal.Any() ? typesFinal : null,
            Entities = entitiesFinal.Any() ? entitiesFinal : null,
        };
    }
}
