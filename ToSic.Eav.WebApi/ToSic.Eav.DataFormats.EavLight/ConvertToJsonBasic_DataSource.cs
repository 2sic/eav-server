using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using static System.String;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataFormats.EavLight
{
    /// <summary>
    /// A helper to serialize various combinations of entities, lists of entities etc
    /// </summary>
    [PrivateApi("Hide implementation")]
    public partial class ConvertToEavLight
    {
        ///// <summary>
        ///// Important: this constructor is used both in inherited,
        ///// but also in EAV-code which uses only this object (so no inherited)
        ///// This is why it must be public, because otherwise it can't be constructed from eav?
        ///// </summary>
        ///// <param name="dependencies"></param>
        //public ConvertToJsonBasic(Dependencies dependencies): base(dependencies, "Eav.CnvE2D") { }

        #region Many variations of the Prepare-Statement expecting various kinds of input

        /// <inheritdoc />
        public IDictionary<string, IEnumerable<EavLightEntity>> Convert(IDataSource source, IEnumerable<string> streams = null)
            => Convert(source, streams, null);
        
        [PrivateApi("not public yet, as the signature is not final yet")]
        public IDictionary<string, IEnumerable<EavLightEntity>> Convert(IDataSource source, IEnumerable<string> streams, string[] filterGuids)
        {
            var wrapLog = Log.Fn<IDictionary<string, IEnumerable<EavLightEntity>>>(startTimer: true);
            string[] streamsList;
            if (streams != null)
            {
                Log.A("Will use provided list of streams.");
                streamsList = streams.ToArray();
            }
            else
            {
                Log.A("No streams specified, will create list with all names.");
                streamsList = source.Out.Select(p => p.Key).ToArray();
            }

            Log.A("Streams: " + Join(",", streamsList));

            // pre-process the guids list to ensure they are guids
            var realGuids = Array.Empty<Guid>();
            if (filterGuids?.Length > 0)
                realGuids = filterGuids
                    .Select(g => Guid.TryParse(g, out var validGuid) ? validGuid as Guid? : null)
                    .Where(g => g != null)
                    .Cast<Guid>()
                    .ToArray();


            var y = streamsList
                .Where(k => source.Out.ContainsKey(k))
                .ToDictionary(
                    k => k,
                    s =>
                    {
                        var list = source.Out[s].List;
                        if (realGuids.Length > 0)
                            list = list.Where(e => realGuids.Contains(e.EntityGuid));
                        return Convert(list);
                    });

            return wrapLog.ReturnAsOk(y);
        }

        /// <inheritdoc />
        public IDictionary<string, IEnumerable<EavLightEntity>> Convert(IDataSource source, string streams)
            => Convert(source, streams?.Split(','));


        #endregion

        
    }
}