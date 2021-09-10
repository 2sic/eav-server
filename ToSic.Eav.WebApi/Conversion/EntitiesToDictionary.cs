using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
#if NET451
using System.Web.Http;
using Newtonsoft.Json;
#endif
using ToSic.Eav.DataSources;
using ToSic.Eav.Documentation;
using ToSic.Eav.ImportExport.Convert.EntityToDictionaryLight;
using static System.String;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Conversion
{
    /// <summary>
    /// A helper to serialize various combinations of entities, lists of entities etc
    /// </summary>
    [PrivateApi("Made private in v12.04, as it shouldn't be used in razor - but previously it was in some apps so we must assume it's in use")]
    public class EntitiesToDictionary: EntitiesToDictionaryBase, IConvertStreams<IDictionary<string, object>>
    {
        /// <summary>
        /// Important: this constructor is used both in inherited,
        /// but also in EAV-code which uses only this object (so no inherited)
        /// This is why it must be public, because otherwise it can't be constructed from eav?
        /// </summary>
        /// <param name="dependencies"></param>
        public EntitiesToDictionary(Dependencies dependencies): base(dependencies, "Eav.CnvE2D") { }

#if NETFRAMEWORK
        /// <summary>
        /// Old constructor used in some public apps in razor, so it must remain for DNN implementation
        /// But .net 451 only!
        /// In .net .451 it must also set the formatters to use the right date-time, which isn't necessary in .net core. 
        /// </summary>
        /// <remarks>
        /// has an important side effect, this isn't clear from outside!
        /// </remarks>
        [Obsolete]
        public EntitiesToDictionary(): this(Factory.ObsoleteBuild<Dependencies>())
        {
            // Ensure that date-times are sent in the Zulu-time format (UTC) and not with offsets which causes many problems during round-trips
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            // #DoneDotNetStandard - there it's handled in the startup.cs
        }
#endif


        #region Many variations of the Prepare-Statement expecting various kinds of input

        /// <inheritdoc />
        public IDictionary<string, IEnumerable<IDictionary<string, object>>> Convert(IDataSource source, IEnumerable<string> streams = null)
            => Convert(source, streams, null);
        
        [PrivateApi("not public yet, as the signature is not final yet")]
        public IDictionary<string, IEnumerable<IDictionary<string, object>>> Convert(IDataSource source, IEnumerable<string> streams, string[] onlyTheseGuids)
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
            if (onlyTheseGuids?.Length > 0)
                realGuids = onlyTheseGuids
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
        public IDictionary<string, IEnumerable<IDictionary<string, object>>> Convert(IDataSource source, string streams)
            => Convert(source, streams?.Split(','));

        /// <inheritdoc />
        public IEnumerable<IDictionary<string, object>> Convert(IDataStream stream)
            => Convert(stream.List);
        
        
        #endregion

        
    }
}