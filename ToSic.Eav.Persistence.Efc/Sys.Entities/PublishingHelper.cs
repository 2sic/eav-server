﻿using System.Diagnostics.CodeAnalysis;
using ToSic.Eav.Persistence.Efc.Sys.Services;

namespace ToSic.Eav.Persistence.Efc.Sys.Entities;

internal class PublishingHelper(EfcAppLoaderService parent): HelperBase(parent.Log, "Efc.PubHlp")
{
    [field: AllowNull, MaybeNull]
    internal EntityQueries EntityQueries => field ??= new(parent.Context, Log);

    public int[] AddEntityIdOfPartnerEntities(int[] publishedIds)
    {
        var l = Log.Fn<int[]>(timer: true);

        var relatedIds = EntityQueries
            .EntitiesOfAdditionalDrafts(publishedIds)
            .Select(e => e.PublishedEntityId!.Value)
            .ToListOpt();

        var combined = Enumerable.Union(publishedIds, relatedIds)
            .ToArray();

        return l.ReturnAsOk(combined);
    }

}