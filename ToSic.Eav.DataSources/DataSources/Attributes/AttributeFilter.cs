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
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Attribute Remover",
        UiHint = "Remove attributes/properties to limit what is available",
        Icon = "delete_sweep",
        Type = DataSourceType.Modify, 
        GlobalName = "ToSic.Eav.DataSources.AttributeFilter, ToSic.Eav.DataSources",
        DynamicOut = false,
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.AttributeFilter",
        HelpLink = "https://r.2sxc.org/DsAttributeFilter")]

    public class AttributeFilter : DataSourceBase
	{
        #region Constants

        public static string KeepAll = "*";

        #endregion
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
            Configuration.Parse();

            var raw = AttributeNames;
            // note: since 2sxc 11.13 we have lines for attributes
            // older data still uses commas since it was single-line
            var attributeNames = raw.Split(raw.Contains("\n") ? '\n' : ',');


            attributeNames = attributeNames
                .Select(a => a.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
            
            // If no attributes were given or just one with *, then don't filter at all
            var keepAll = attributeNames.Length == 0 || (attributeNames.Length == 1 && attributeNames[0] == KeepAll);

            var result = In[Constants.DefaultStreamName].Immutable
                .Select(entity =>
                {
                    var attributes = entity.Attributes;
                    if (!keepAll)
                        attributes = attributes.Where(a => attributeNames.Contains(a.Key))
                            .ToDictionary(k => k.Key, v => v.Value);
                    return EntityBuilder.FullClone(entity, attributes, entity.Relationships.AllRelationships);
                })
                .Cast<IEntity>()
                .ToImmutableArray();

            Log.Add($"attrib filter names:[{string.Join(",", attributeNames)}] found:{result.Length}");
		    return result;
		}
        
	}
}