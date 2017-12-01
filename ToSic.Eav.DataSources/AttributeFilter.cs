using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources.VisualQuery;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// DataSource to only pass through configured AttributeNames
	/// </summary>
	/// <remarks>Uses Configuration "AttributeNames"</remarks>

	[VisualQuery(Type = DataSourceType.Modify, DynamicOut = false,
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-AttributeFilter")]

    public class AttributeFilter : BaseDataSource
	{
		#region Configuration-properties
		private const string AttributeNamesKey = "AttributeNames";
	    public override string LogId => "DS.AtribF";

        /// <summary>
        /// A string containing one or more entity-ids. like "27" or "27,40,3063,30306"
        /// </summary>
        public string AttributeNames
		{
		    get => Configuration[AttributeNamesKey];
            set => Configuration[AttributeNamesKey] = value;
        }

		#endregion

		/// <inheritdoc />
		/// <summary>
		/// Constructs a new AttributeFilter DataSource
		/// </summary>
		public AttributeFilter()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetList));
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

		    var result = In[Constants.DefaultStreamName].List
                .Select(entity => EntityBuilder.FullClone(entity, 
                    entity.Attributes.Where(a => attributeNames.Contains(a.Key)).ToDictionary(k => k.Key, v => v.Value),
                    (entity.Relationships as RelationshipManager).AllRelationships)).Cast<IEntity>().ToList();

		    Log.Add($"attrib filter names:[{string.Join(",", attributeNames)}] found:{result.Count}");
		    return result;
		}

        /// <inheritdoc />
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