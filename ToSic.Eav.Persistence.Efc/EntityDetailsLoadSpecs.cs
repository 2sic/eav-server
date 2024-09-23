using ToSic.Eav.Generics;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Persistence.Efc.Intermediate;

namespace ToSic.Eav.Persistence.Efc;

internal class EntityDetailsLoadSpecs(int appId, int[] entityIds, List<TempEntity> entities, IEavFeaturesService features, ILog parentLog): HelperBase(parentLog, "Sql.DetLSp")
{
    public int AppId => appId;

    private List<int> IdsToLoad
    {
        get
        {
            if (_entityIds != null) return _entityIds;

            var l = Log.Fn<List<int>>(timer: true);
            // Get the EntityIDs to load the relationships / values for these entities
            // But skip all entities which have JSON, since for these all the data is already loaded
            // This pre-clean can dramatically reduce the time needed to load the data
            var entityIdSource = features.IsEnabled(BuiltInFeatures.SqlLoadPerformance)
                ? entities.Where(e => e.Json == null)
                : entities;
            _entityIds = entityIdSource
                .Where(e => e.Json == null)
                .Select(e => e.EntityId)
                .ToList();

            return l.Return(_entityIds, $"Total entities: {entities.Count}; final IDs: {_entityIds.Count}");
        }
    }
    private List<int> _entityIds;

    public List<List<int>> IdsToLoadChunks
    {
        get
        {
            if (_entityIdsChunks != null) return _entityIdsChunks;
            var l = Log.Fn<List<List<int>>>(timer: true);
            _entityIdsChunks = IdsToLoad.ChunkBy(EntityLoader.IdChunkSize);
            return l.Return(_entityIdsChunks, $"Chunked into {_entityIdsChunks.Count} chunks");
        }
    }

    private List<List<int>> _entityIdsChunks;

}