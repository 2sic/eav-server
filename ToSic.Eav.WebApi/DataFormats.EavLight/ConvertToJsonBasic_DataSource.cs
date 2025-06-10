using ToSic.Eav.DataSource;
using ToSic.Lib.Documentation;
using static System.String;

namespace ToSic.Eav.DataFormats.EavLight;

/// <summary>
/// A helper to serialize various combinations of entities, lists of entities etc
/// </summary>
[PrivateApi("Hide implementation")]
partial class ConvertToEavLight
{

    #region Many variations of the Prepare-Statement expecting various kinds of input

    /// <inheritdoc cref="IConvertDataSource{T}.Convert" />
    public IDictionary<string, IEnumerable<EavLightEntity>> Convert(IDataSource source, IEnumerable<string> streams = null)
        => Convert(source, streams, null);
        
    [PrivateApi("not public yet, as the signature is not final yet")]
    public IDictionary<string, IEnumerable<EavLightEntity>> Convert(IDataSource source, IEnumerable<string>? streams, string[]? filterGuids)
    {
        var l = Log.Fn<IDictionary<string, IEnumerable<EavLightEntity>>>(timer: true);

        var streamsList = streams?.ToArray()
            ?? source.Out.Select(p => p.Key).ToArray();

        var msg = streams != null
            ? "Will use provided list of streams."
            : "No streams specified, will create list with all names.";
        l.A($"{msg} Streams: {Join(",", streamsList)}");

        // Pre-process the guids list to ensure they are guids
        var realGuids = filterGuids?
                            .Select(str => Guid.TryParse(str, out var guid) ? guid : Guid.Empty)
                            .Where(guid => guid != Guid.Empty)
                            .ToArray()
                        ?? [];


        var allStreams = streamsList
            .Where(source.Out.ContainsKey)
            .ToDictionary(
                k => k,
                s =>
                {
                    var list = source.Out[s].List.ToListOpt();
                    var filtered = realGuids.Any()
                        ? list.Where(e => realGuids.Contains(e.EntityGuid))
                        : list;
                    return Convert(filtered);
                });

        return l.ReturnAsOk(allStreams);
    }

    /// <inheritdoc cref="IConvertDataSource{T}.Convert" />
    public IDictionary<string, IEnumerable<EavLightEntity>> Convert(IDataSource source, string streams)
        => Convert(source, streams?.Split(','));


    #endregion

        
}