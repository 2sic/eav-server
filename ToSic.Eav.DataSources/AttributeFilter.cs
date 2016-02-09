using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// DataSource to only pass through configured AttributeNames
	/// </summary>
	/// <remarks>Uses Configuration "AttributeNames"</remarks>
	[PipelineDesigner]
	public class AttributeFilter : BaseDataSource
	{
		#region Configuration-properties
		private const string AttributeNamesKey = "AttributeNames";

		/// <summary>
		/// A string containing one or more entity-ids. like "27" or "27,40,3063,30306"
		/// </summary>
		public string AttributeNames
		{
		    get { return Configuration[AttributeNamesKey]; }
		    set { Configuration[AttributeNamesKey] = value; }

		}

		#endregion

		/// <summary>
		/// Constructs a new AttributeFilter DataSource
		/// </summary>
		public AttributeFilter()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetList));
			Configuration.Add(AttributeNamesKey, "[Settings:AttributeNames]");

            CacheRelevantConfigurations = new[] { AttributeNamesKey };
        }

        /// <summary>
        /// Get the list of all items with reduced attributes-list
        /// </summary>
        /// <returns></returns>
		private IEnumerable<IEntity> GetList()
		{
			EnsureConfigurationIsLoaded();

		    var attributeNames = AttributeNames.Split(',');
		    attributeNames = (from a in attributeNames select a.Trim()).ToArray();

		    return In[Constants.DefaultStreamName].LightList
                .Select(entity => new Data.Entity(entity, 
                    entity.Attributes.Where(a => attributeNames.Contains(a.Key)).ToDictionary(k => k.Key, v => v.Value),
                    (entity.Relationships as RelationshipManager).AllRelationships)).Cast<IEntity>().ToList();
		}

        /// <summary>
        /// Load configuration and normalize parameters AttributeNames
        /// </summary>
	    protected internal override void EnsureConfigurationIsLoaded()
	    {
	        base.EnsureConfigurationIsLoaded();

            var namesList = (from a in AttributeNames.Split(',') select a.Trim()).ToArray();
            AttributeNames = string.Join(",", namesList.ToArray());
	    }
	}
}