using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using ToSic.Eav.Conversion;
using ToSic.Eav.DataSources;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Serialization
{
    /// <summary>
    /// A helper to serialize various combinations of entities, lists of entities etc
    /// </summary>
    public class EntitiesToDictionary: EntitiesToDictionaryBase, IStreamsTo<Dictionary<string, object>>
    {
        // TODO: has an important side effect, this isn't clear from outside!
        public EntitiesToDictionary()
        {
            // Ensure that date-times are sent in the Zulu-time format (UTC) and not with offsets which causes many problems during round-trips
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
        }
        

        #region Many variations of the Prepare-Statement expecting various kinds of input
        /// <summary>
        /// Returns an object that represents an IDataSource, but is serializable. If streamsToPublish is null, it will return all streams.
        /// </summary>
        public Dictionary<string, IEnumerable<Dictionary<string, object>>> Convert(IDataSource source, IEnumerable<string> streams = null)
        {
            if (streams == null)
                streams = source.Out.Select(p => p.Key);

            var y = streams.Where(k => source.Out.ContainsKey(k))
                .ToDictionary(k => k, s => source.Out[s].List.Select(GetDictionaryFromEntity)
            );

            return y;
        }

        /// <summary>
        /// Returns an object that represents an IDataSource, but is serializable. If streamsToPublish is null, it will return all streams.
        /// </summary>
        /// <remarks>
        ///     note that this could be in use on webAPIs and scripts
        ///     so even if it looks un-used, it must stay available
        /// </remarks>
        public Dictionary<string, IEnumerable<Dictionary<string, object>>> Convert(IDataSource source, string streams)
            => Convert(source, streams.Split(','));

        /// <summary>
        /// Return an object that represents an IDataStream, but is serializable
        /// </summary>
        /// <remarks>
        ///     note that this could be in use on webAPIs and scripts
        ///     so even if it looks un-used, it must stay available
        /// </remarks>
        public IEnumerable<Dictionary<string, object>> Convert(IDataStream stream)
            => Convert(stream.List);
        
        
        #endregion

        
    }
}