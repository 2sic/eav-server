using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.Documentation;
using ToSic.Eav.ImportExport.JsonLight;
using static System.String;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Convert
{
    /// <summary>
    /// A helper to serialize various combinations of entities, lists of entities etc
    /// </summary>
    [PrivateApi("Hide implementation")]
    public partial class ConvertToJsonLight
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
        public IDictionary<string, IEnumerable<JsonEntity>> Convert(IDataSource source, IEnumerable<string> streams = null)
            => Convert(source, streams, null);
        
        [PrivateApi("not public yet, as the signature is not final yet")]
        public IDictionary<string, IEnumerable<JsonEntity>> Convert(IDataSource source, IEnumerable<string> streams, string[] filterGuids)
        {
            var wrapLog = Log.Call(useTimer: true);
            string[] streamsList;
            if (streams != null)
            {
                Log.Add("Will use provided list of streams.");
                streamsList = streams.ToArray();
            }
            else
            {
                Log.Add("No streams specified, will create list with all names.");
                streamsList = source.Out.Select(p => p.Key).ToArray();
            }

            Log.Add("Streams: ", Join(",", streamsList));

            // pre-process the guids list to ensure they are guids
            var realGuids = new Guid[0];
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

            wrapLog("ok");
            return y;
        }

        /// <inheritdoc />
        public IDictionary<string, IEnumerable<JsonEntity>> Convert(IDataSource source, string streams)
            => Convert(source, streams?.Split(','));


        #endregion

        
    }
}