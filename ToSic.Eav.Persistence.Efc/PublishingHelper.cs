namespace ToSic.Eav.Persistence.Efc;

internal class PublishingHelper(Efc11Loader parent): HelperBase(parent.Log, "Efc.PubHlp")
{
    internal EntityQueries EntityQueries => _entityQueries ??= new(parent.Context, Log);
    private EntityQueries _entityQueries;

    public int[] AddEntityIdOfPartnerEntities(int[] publishedIds)
    {
        var l = Log.Fn<int[]>(timer: true);

        var relatedIds = EntityQueries
            .EntitiesOfAdditionalDrafts(publishedIds)
            .Select(e => e.PublishedEntityId.Value);

        var combined = publishedIds
            .Union(relatedIds)
            .ToArray();

        return l.ReturnAsOk(combined);
    }

}