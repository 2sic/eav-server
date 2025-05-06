using ToSic.Eav.Internal.Features;
using ToSic.Eav.Internal.Generics;
using ToSic.Eav.Persistence.Efc.Intermediate;

namespace ToSic.Eav.Persistence.Efc;

internal class EntityDetailsLoadSpecs(int appId, List<TempEntity> entities, IEavFeaturesService features, ILog parentLog): HelperBase(parentLog, "Sql.DetLSp")
{
    public int AppId => appId;

    private List<int> IdsToLoad
    {
        get
        {
            if (field != null)
                return field;

            var l = Log.Fn<List<int>>(timer: true);
            // Get the EntityIDs to load the relationships / values for these entities
            // But skip all entities which have JSON, since for these all the data is already loaded
            // This pre-clean can dramatically reduce the time needed to load the data
            var entityIdSource = features.IsEnabled(BuiltInFeatures.SqlLoadPerformance)
                ? entities.Where(e => e.Json == null)
                : entities;
            field = entityIdSource
                .Where(e => e.Json == null)
                .Select(e => e.EntityId)
                .ToList();

            return l.Return(field, $"Total entities: {entities.Count}; final IDs: {field.Count}");
        }
    }

    public List<List<int>> IdsToLoadChunks
    {
        get
        {
            if (field != null)
                return field;

            var l = Log.Fn<List<List<int>>>(timer: true);
            field = IdsToLoad.ChunkBy(EntityLoader.IdChunkSize);
            return l.Return(field, $"Chunked into {field.Count} chunks");
        }
    }
}