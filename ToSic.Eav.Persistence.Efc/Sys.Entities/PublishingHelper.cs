using System.Diagnostics.CodeAnalysis;
using ToSic.Eav.Persistence.Efc.Sys.Services;

namespace ToSic.Eav.Persistence.Efc.Sys.Entities;

internal class PublishingHelper(EfcAppLoaderService appLoader): HelperBase(appLoader.Log, "Efc.PubHlp")
{
    [field: AllowNull, MaybeNull]
    internal EntityQueries EntityQueries => field ??= new(appLoader.Context, Log);

    public int[] AddEntityIdOfPartnerEntities(int[] publishedIds)
    {
        var l = Log.IfDetails(appLoader.LogSettings).Fn<int[]>(timer: true);

        var relatedIds = EntityQueries
            .EntitiesOfAdditionalDrafts(publishedIds)
            .Select(e => e.PublishedEntityId!.Value)
            .ToListOpt();

        var combined = Enumerable.Union(publishedIds, relatedIds)
            .ToArray();

        return l.ReturnAsOk(combined);
    }

}