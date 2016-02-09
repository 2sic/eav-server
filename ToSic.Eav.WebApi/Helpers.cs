using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.WebApi
{
	/// <summary>
	/// Helpers for DataSources
	/// </summary>
	public class Helpers
	{
		#region Helpers for JSON

		/// <summary>
		/// Get Entity Values in a Dictionary
		/// </summary>
		public static Dictionary<string, object> GetEntityValues(IEntity entity, int dimensionId = 0, string cultureCode = null, bool addEntityId = true, bool addEntityGuid = true)
		{
			var attributes = entity.Attributes.ToDictionary(k => k.Value.Name, v => (cultureCode == null ? v.Value[dimensionId] : v.Value[cultureCode]));
			if (addEntityId)
				attributes.Add("EntityId", entity.EntityId);
			if (addEntityGuid)
				attributes.Add("EntityGuid", entity.EntityGuid);
			return attributes;
		}

		/// <summary>
		/// Get Entities with their Values in a Dictionary
		/// </summary>
		public static IEnumerable<Dictionary<string, object>> GetEntityValues(IEnumerable<IEntity> entities, int dimensionId = 0, bool addEntityId = true, bool addEntityGuid = true)
		{
			return entities.Select(e => GetEntityValues(e, addEntityId: addEntityId, addEntityGuid: addEntityGuid, dimensionId: dimensionId));
		}

		/// <summary>
		/// Get Streams in a simple JSON format
		/// </summary>
		/// <param name="streams">Dictionary with Key = Name of the Stream in Output, Value = an IDataStream or <see cref="IEnumerable{IEntity}"/> or a single IEntity</param>
		/// <param name="addEntityId">Indicates whether Result should contan EntityId</param>
		/// <param name="addEntityGuid">Indicates whether Result should contan EntityGuid</param>
		/// <returns>Dictionary that can be parsed as JSON using System.Web.Helpers.Json.Encode()</returns>
		public static Dictionary<string, IEnumerable<Dictionary<string, object>>> GetStreamsForJson(IDictionary<string, IDataStream> streams, bool addEntityId = true, bool addEntityGuid = true)
		{
			var result = new Dictionary<string, IEnumerable<Dictionary<string, object>>>();
			foreach (var stream in streams)
			{
				var entities = stream.Value.List.Select(e => GetEntityValues(e.Value, addEntityId: addEntityId, addEntityGuid: addEntityGuid));
				result.Add(stream.Key, entities);
			}

			return result;
		}

		#endregion
	}
}