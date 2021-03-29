using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
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
        NiceName = "Remove Attribute/Property",
        UiHint = "Remove attributes/properties to limit what is available",
        Icon = "delete_sweep",
        Type = DataSourceType.Modify, 
        GlobalName = "ToSic.Eav.DataSources.AttributeFilter, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new []{Constants.DefaultStreamName},
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.AttributeFilter",
        HelpLink = "https://r.2sxc.org/DsAttributeFilter")]

    public class AttributeFilter : DataSourceBase
	{
        #region Constants

        public static string ModeKeep = "+";
        public static string ModeRemove = "-";

        #endregion
        
        #region Configuration-properties
        
        private const string AttributeNamesKey = "AttributeNames";
        private const string ModeKey = "Mode";

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
        
        /// <summary>
        /// A string containing one or more attribute names. like "FirstName" or "FirstName,LastName,Birthday"
        /// </summary>
        public string Mode
		{
		    get => Configuration[ModeKey];
            set => Configuration[ModeKey] = value;
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
			ConfigMask(AttributeNamesKey, $"[Settings:{AttributeNamesKey}]");
            ConfigMask(ModeKey, $"[Settings:{ModeKey}||+]");
        }

        /// <summary>
        /// Get the list of all items with reduced attributes-list
        /// </summary>
        /// <returns></returns>
		private IImmutableList<IEntity> GetList()
        {
            var wrapLog = Log.Call<IImmutableList<IEntity>>();
            Configuration.Parse();

            var raw = AttributeNames;
            // note: since 2sxc 11.13 we have lines for attributes
            // older data still uses commas since it was single-line
            var attributeNames = raw.Split(raw.Contains("\n") ? '\n' : ',');
            attributeNames = attributeNames
                .Select(a => a.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
            
            Log.Add($"attrib filter names:[{string.Join(",", attributeNames)}]");
            
            // Determine if we should remove or keep the things in the list
            var keepNamedAttributes = Mode != ModeRemove;
            
            // If no attributes were given or just one with *, then don't filter at all
            var noFieldNames = attributeNames.Length == 0 
                          || attributeNames.Length == 1 && string.IsNullOrWhiteSpace(attributeNames[0]);

            if (GetStreamOrPrepareExceptionToThrow(Constants.DefaultStreamName, out var sourceList)) 
                return wrapLog("error", sourceList);

            // Case #1 if we don't change anything, short-circuit and return original
            if (noFieldNames && !keepNamedAttributes)
                return wrapLog($"keep original {sourceList.Count}", sourceList);

            var result = sourceList
                .Select(e =>
                {
                    // Case 2: Check if we should take none at all
                    if (noFieldNames && keepNamedAttributes)
                        return EntityBuilder.FullClone(e, new Dictionary<string, IAttribute>(), null);

                    // Case 3 - not all fields, keep/drop the ones we don't want
                    var attributes = e.Attributes
                        .Where(a => attributeNames.Contains(a.Key) == keepNamedAttributes)
                        .ToDictionary(k => k.Key, v => v.Value);
                    return EntityBuilder.FullClone(e, attributes, e.Relationships.AllRelationships);
                })
                .Cast<IEntity>()
                .ToImmutableList();

		    return wrapLog($"modified {result.Count}", result);
		}
        
    }
}