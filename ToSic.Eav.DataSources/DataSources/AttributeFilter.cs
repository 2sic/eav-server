using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources.Query;
using ToSic.Eav.Documentation;
using ToSic.Eav.Interfaces;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// DataSource to only pass through configured AttributeNames - other attributes/properties are removed from the entities.
	/// </summary>
	/// <remarks>Uses Configuration "AttributeNames"</remarks>
    [PublicApi]
	[Query.VisualQuery(GlobalName = "ToSic.Eav.DataSources.AttributeFilter, ToSic.Eav.DataSources",
        Type = DataSourceType.Modify, 
        DynamicOut = false,
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.AttributeFilter",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-AttributeFilter")]

    public class AttributeFilter : DataSourceBase
	{
		#region Configuration-properties
		private const string AttributeNamesKey = "AttributeNames";

        /// <inheritdoc/>
        [PrivateApi]
        public override string LogId => "DS.AtribF";

        /// <summary>
        /// A string containing one or more attribute names. like "FirstName" or "FirstName,LastName,Birthday"
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
		[PrivateApi]
		public AttributeFilter()
		{
            Provide(GetList);
			ConfigMask(AttributeNamesKey, "[Settings:AttributeNames]");
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
        [PrivateApi]
        protected internal override void EnsureConfigurationIsLoaded()
	    {
	        base.EnsureConfigurationIsLoaded();

            var namesList = (from a in AttributeNames.Split(',') select a.Trim()).ToArray();
            AttributeNames = string.Join(",", namesList.ToArray());
	    }
	}
}