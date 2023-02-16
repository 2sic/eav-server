using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
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
        Icon = Icons.Delete,
        Type = DataSourceType.Modify, 
        GlobalName = "ToSic.Eav.DataSources.AttributeFilter, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new []{Constants.DefaultStreamNameRequired},
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.AttributeFilter",
        HelpLink = "https://r.2sxc.org/DsAttributeFilter")]

    public class AttributeFilter : DataSource
	{
        #region Constants

        [PrivateApi] public const string ModeKeep = "+";
        [PrivateApi] public const string ModeRemove = "-";

        #endregion
        
        #region Configuration-properties
        
        /// <summary>
        /// A string containing one or more attribute names. like "FirstName" or "FirstName,LastName,Birthday"
        /// </summary>
        [Configuration]
        public string AttributeNames
		{
		    get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }
        
        /// <summary>
        /// A string containing one or more attribute names. like "FirstName" or "FirstName,LastName,Birthday"
        /// </summary>
        [Configuration(Fallback = ModeKeep)]
        public string Mode
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }
      

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new AttributeFilter DataSource
        /// </summary>
        [PrivateApi]
		public AttributeFilter(EntityBuilder entityBuilder, Dependencies services): base(services, $"{DataSourceConstants.LogPrefix}.AtribF")
        {
            _entityBuilder = entityBuilder;
            Provide(GetList);
        }

        private readonly EntityBuilder _entityBuilder;


        /// <summary>
        /// Get the list of all items with reduced attributes-list
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> GetList() => Log.Func(l =>
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

            l.A($"attrib filter names:[{string.Join(",", attributeNames)}]");

            // Determine if we should remove or keep the things in the list
            var keepNamedAttributes = Mode != ModeRemove;

            // If no attributes were given or just one with *, then don't filter at all
            var noFieldNames = attributeNames.Length == 0
                               || attributeNames.Length == 1 && string.IsNullOrWhiteSpace(attributeNames[0]);

            if (!GetRequiredInList(out var sourceList))
                return (sourceList, "error");

            // Case #1 if we don't change anything, short-circuit and return original
            if (noFieldNames && !keepNamedAttributes)
                return (sourceList, $"keep original {sourceList.Count}");

            var result = sourceList
                .Select(e =>
                {
                    // Case 2: Check if we should take none at all
                    if (noFieldNames && keepNamedAttributes)
                        return _entityBuilder.Clone(e, new Dictionary<string, IAttribute>(), null);

                    // Case 3 - not all fields, keep/drop the ones we don't want
                    var attributes = e.Attributes
                        .Where(a => attributeNames.Contains(a.Key) == keepNamedAttributes)
                        .ToDictionary(k => k.Key, v => v.Value);
                    return _entityBuilder.Clone(e, attributes, e.Relationships.AllRelationships);
                })
                .Cast<IEntity>()
                .ToImmutableList();

            return (result, $"modified {result.Count}");
        });

    }
}