using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Newtonsoft.Json;
using ToSic.Eav.DataSources;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Serialization
{
    /// <inheritdoc />
    /// <summary>
    /// A helper to serialize various combinations of entities, lists of entities etc
    /// </summary>
    public class Serializer: EntityToDictionary
    {

        public Serializer()
        {
            // Ensure that date-times are sent in the Zulu-time format (UTC) and not with offsets which causes many problems during round-trips
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
        }
        

        #region Many variations of the Prepare-Statement expecting various kinds of input
        /// <summary>
        /// Returns an object that represents an IDataSource, but is serializable. If streamsToPublish is null, it will return all streams.
        /// </summary>
        public Dictionary<string, IEnumerable<Dictionary<string, object>>> Prepare(IDataSource source, IEnumerable<string> streamsToPublish = null)
        {
            if (streamsToPublish == null)
                streamsToPublish = source.Out.Select(p => p.Key);

            var y = streamsToPublish.Where(k => source.Out.ContainsKey(k))
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
        public Dictionary<string, IEnumerable<Dictionary<string, object>>> Prepare(IDataSource source, string streamsToPublish)
            => Prepare(source, streamsToPublish.Split(','));

        /// <summary>
        /// Return an object that represents an IDataStream, but is serializable
        /// </summary>
        /// <remarks>
        ///     note that this could be in use on webAPIs and scripts
        ///     so even if it looks un-used, it must stay available
        /// </remarks>
        public IEnumerable<Dictionary<string, object>> Prepare(IDataStream stream)
            => Convert(stream.List);
        
        
        #endregion

        
    }
}