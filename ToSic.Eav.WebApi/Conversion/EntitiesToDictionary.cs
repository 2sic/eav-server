using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
#if NET451
using System.Web.Http;
using Newtonsoft.Json;
#endif
using ToSic.Eav.DataSources;
using ToSic.Eav.Documentation;
using static System.String;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Conversion
{
    /// <summary>
    /// A helper to serialize various combinations of entities, lists of entities etc
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public class EntitiesToDictionary: EntitiesToDictionaryBase, IStreamsTo<Dictionary<string, object>>
    {
        // TODO: has an important side effect, this isn't clear from outside!
        public EntitiesToDictionary(): base(Factory.Resolve<IValueConverter>(), Factory.Resolve<IZoneCultureResolver>(), "Cnv.Ent2Dc")
        {
            // Ensure that date-times are sent in the Zulu-time format (UTC) and not with offsets which causes many problems during round-trips
#if NET451
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
#else
            // #DoneDotNetStandard - there it's handled in the startup.cs
#endif
        }


        #region Many variations of the Prepare-Statement expecting various kinds of input

        /// <inheritdoc />
        public Dictionary<string, IEnumerable<Dictionary<string, object>>> Convert(IDataSource source, IEnumerable<string> streams = null)
        {
            var wrapLog = Log.Call(useTimer: true);
            string[] streamsList;
            if (streams != null)
            {
                streamsList = streams.ToArray();
                Log.Add("Will use provided list of streams.");
            }
            else
            {
                Log.Add("No streams specified, will create list with all names.");
                streamsList = source.Out.Select(p => p.Key).ToArray();
            }

            Log.Add("Streams: ", Join(",", streamsList));

            var y = streamsList
                .Where(k => source.Out.ContainsKey(k))
                .ToDictionary(
                    k => k,
                    s => Convert(source.Out[s]) // .Select(GetDictionaryFromEntity)
                );

            wrapLog("ok");
            return y;
        }

        /// <inheritdoc />
        public Dictionary<string, IEnumerable<Dictionary<string, object>>> Convert(IDataSource source, string streams)
            => Convert(source, streams?.Split(','));

        /// <inheritdoc />
        public IEnumerable<Dictionary<string, object>> Convert(IDataStream stream)
            => Convert(stream.List);
        
        
        #endregion

        
    }
}