using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// DataSource to only pass through configured AttributeNames - other attributes/properties are removed from the entities.
	/// </summary>
	/// <remarks>Uses Configuration "AttributeNames"</remarks>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.AttributeFilter, ToSic.Eav.DataSources",
        Type = DataSourceType.Modify, 
        DynamicOut = false,
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.AttributeFilter",
        HelpLink = "https://r.2sxc.org/DsAttributeFilter")]

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
		private ImmutableArray<IEntity> GetList()
		{
            CustomConfigurationParse();

            var attributeNames = AttributeNames.Split(',');
		    attributeNames = (from a in attributeNames select a.Trim()).ToArray();

		    var result = In[Constants.DefaultStreamName].Immutable
                .Select(entity => EntityBuilder.FullClone(entity, 
                    entity.Attributes.Where(a => attributeNames.Contains(a.Key)).ToDictionary(k => k.Key, v => v.Value),
                    entity.Relationships.AllRelationships)).Cast<IEntity>()
                .ToImmutableArray();
                //.ToList();

		    Log.Add($"attrib filter names:[{string.Join(",", attributeNames)}] found:{result.Length}");
		    return result;
		}

        /// <inheritdoc />
        [PrivateApi]
        private void CustomConfigurationParse()
	    {
            Configuration.Parse();

            var namesList = (from a in AttributeNames.Split(',') select a.Trim()).ToArray();
            AttributeNames = string.Join(",", namesList.ToArray());
	    }
	}
}