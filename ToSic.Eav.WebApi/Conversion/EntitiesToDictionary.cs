using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using ToSic.Eav.DataSources;
using ToSic.Eav.Documentation;

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
        public EntitiesToDictionary()
        {
            // Ensure that date-times are sent in the Zulu-time format (UTC) and not with offsets which causes many problems during round-trips
#if NET451
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
#else
            // "Not Yet Implemented in .net standard #TodoNetStandard";
            // probably do at top level like https://stackoverflow.com/questions/58102189/formatting-datetime-in-asp-net-core-3-0-using-system-text-json
#endif
        }


        #region Many variations of the Prepare-Statement expecting various kinds of input
        /// <inheritdoc />
        public Dictionary<string, IEnumerable<Dictionary<string, object>>> Convert(IDataSource source, IEnumerable<string> streams = null)
        {
            if (streams == null)
                streams = source.Out.Select(p => p.Key);

            var y = streams.Where(k => source.Out.ContainsKey(k))
                .ToDictionary(k => k, s => source.Out[s].List.Select(GetDictionaryFromEntity)
            );

            return y;
        }

        /// <inheritdoc />
        public Dictionary<string, IEnumerable<Dictionary<string, object>>> Convert(IDataSource source, string streams)
            => Convert(source, streams.Split(','));

        /// <inheritdoc />
        public IEnumerable<Dictionary<string, object>> Convert(IDataStream stream)
            => Convert(stream.List);
        
        
        #endregion

        
    }
}