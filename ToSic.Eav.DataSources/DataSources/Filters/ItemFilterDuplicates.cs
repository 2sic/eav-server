﻿using ToSic.Eav.DataSource.Sys;
using ToSic.Eav.DataSource.Sys.Streams;
using static ToSic.Eav.DataSource.DataSourceConstants;


namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// A DataSource that removes duplicate items in a Stream. Often used after a StreamMerge.
/// </summary>
[PublicApi]
[VisualQuery(
    NiceName = "Filter duplicates",
    UiHint = "Remove items which occur multiple times",
    Icon = DataSourceIcons.Filter1,
    Type = DataSourceType.Logic, 
    NameId = "ToSic.Eav.DataSources.ItemFilterDuplicates, ToSic.Eav.DataSources",
    DynamicOut = false,
    In = [StreamDefaultName],
    HelpLink = "https://go.2sxc.org/DsFilterDuplicates")]

public sealed class ItemFilterDuplicates: DataSourceBase
{
    internal const string DuplicatesStreamName = "Duplicates";


    /// <inheritdoc />
    /// <summary>
    /// Constructs a new EntityIdFilter
    /// </summary>
    [PrivateApi]
    public ItemFilterDuplicates(Dependencies services): base(services, $"{DataSourceConstantsInternal.LogPrefix}.Duplic")
    {
        ProvideOut(GetUnique);
        ProvideOut(GetDuplicates, DuplicatesStreamName);
    }

    /// <summary>
    /// Find and return the unique items in the list
    /// </summary>
    /// <returns></returns>
    private IImmutableList<IEntity> GetUnique()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        if (!In.HasStreamWithItems(StreamDefaultName)) 
            return l.Return([], "no in stream with name");

        var source = TryGetIn();
        if (source is null) return l.ReturnAsError(Error.TryGetInFailed());

        var result = source
            .Distinct()
            .ToImmutableOpt();

        return l.Return(result, $"{result.Count}");
    }


    /// <summary>
    /// Find and return only the duplicate items in the list
    /// </summary>
    /// <returns></returns>
    private IImmutableList<IEntity> GetDuplicates()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        if (!In.HasStreamWithItems(StreamDefaultName)) 
            return l.ReturnAsError([], "no in-stream with name");

        var source = TryGetIn();
        if (source is null) return l.ReturnAsError(Error.TryGetInFailed());

        var result = source
            .GroupBy(s => s)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToImmutableOpt();

        return l.Return(result, $"{result.Count}");
    }
}