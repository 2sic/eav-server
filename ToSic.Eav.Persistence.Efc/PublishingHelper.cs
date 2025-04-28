namespace ToSic.Eav.Persistence.Efc;

internal class PublishingHelper(EfcAppLoader parent): HelperBase(parent.Log, "Efc.PubHlp")
{
    internal EntityQueries EntityQueries => field ??= new(parent.Context, Log);

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