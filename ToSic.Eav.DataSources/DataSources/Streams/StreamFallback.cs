﻿using ToSic.Eav.DataSource.Sys;
using ToSic.Eav.DataSource.Sys.Streams;
using static ToSic.Eav.DataSource.DataSourceConstants;


namespace ToSic.Eav.DataSources;

/// <inheritdoc />
/// <summary>
/// A DataSource that returns the first stream which has content
/// </summary>
[PublicApi]
[VisualQuery(
    NiceName = "Stream Fallback",
    UiHint = "Find the first stream which has data",
    Icon = DataSourceIcons.Merge,
    Type = DataSourceType.Logic, 
    NameId = "ToSic.Eav.DataSources.StreamFallback, ToSic.Eav.DataSources",
    DynamicOut = false,
    DynamicIn = true,
    HelpLink = "https://go.2sxc.org/DsStreamFallback")]

public sealed class StreamFallback : DataSourceBase
{

    #region Debug-Properties
    [PrivateApi]
    internal string? DebugReturnedStreamName { get; private set; }
    #endregion


    /// <inheritdoc />
    /// <summary>
    /// Constructs a new EntityIdFilter
    /// </summary>
    [PrivateApi]
    public StreamFallback(Dependencies services) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.FallBk")
    {
        ProvideOut(GetStreamFallback);
    }

    private IImmutableList<IEntity> GetStreamFallback()
    {
        var foundStream = FindIdealFallbackStream();
        return foundStream?.List.ToImmutableOpt() ?? [];
    }

    private IDataStream? FindIdealFallbackStream()
    {
        var l = Log.Fn<IDataStream>("");
        Configuration.Parse();

        // Check if there is a default-stream in with content - if yes, try to return that
        if (In.HasStreamWithItems(StreamDefaultName))
            return l.Return(In[StreamDefaultName], "found default");

        // Otherwise alphabetically assemble the remaining in-streams, try to return those that have content
        var streamList = In
            .Where(x => x.Key != StreamDefaultName)
            .OrderBy(x => x.Key);

        foreach (var stream in streamList)
            if (stream.Value.List.Any())
            {
                DebugReturnedStreamName = stream.Key;
                return l.Return(stream.Value, $"will return stream:{stream.Key}");
            }

        return l.ReturnNull("didn't find any stream, will return empty");
    }
}