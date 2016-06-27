using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// DataSource to rename Attributes
	/// </summary>
	/// <remarks>Uses Configuration "RenamingRules"</remarks>
	[PipelineDesigner]
	public class AttributeRename : BaseDataSource
	{
		#region Configuration-properties
		private const string RenamingRulesKey = "RenamingRules";

		/// <summary>
		/// A string containing one or more entity-ids. like "27" or "27,40,3063,30306"
		/// </summary>
		public IEnumerable<string> RenamingRules
		{
			get
			{
				return string.IsNullOrWhiteSpace(Configuration[RenamingRulesKey])
					? new string[0]
					: Configuration[RenamingRulesKey].Split('\n').Select(p => p.Trim());
			}
			set { Configuration[RenamingRulesKey] = string.Join(",", value); }
		}

		#endregion

		/// <summary>
		/// Constructs a new AttributeRename DataSource
		/// </summary>
		public AttributeRename()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, null, GetList));
			Configuration.Add(RenamingRulesKey, "[Settings:RenamingRules]");

			CacheRelevantConfigurations = new[] { RenamingRulesKey };
		}

		/// <summary>
		/// Get the list of all items with renamed attributes-list
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IEntity> GetList()
		{
			EnsureConfigurationIsLoaded();

			var sourceEntities = In[Constants.DefaultStreamName].LightList;
			var newEntities = new List<IEntity>();

			foreach (var sourceEntity in sourceEntities)
			{
				var attributes = sourceEntity.Attributes;

				// apply all Rename-Rules
				foreach (var rule in RenamingRules)
				{
					string originalName;
					string newName;

					try
					{
						if (string.IsNullOrWhiteSpace(rule))
							continue;

						var ruleParts = rule.Split('=').Select(p => p.Trim()).ToArray();
						originalName = ruleParts[0];
						newName = ruleParts[1];
					}
					catch
					{
						throw new Exception("Error parsing Rename-Rule: " + rule);
					}

					// skip rule if key doesn't exist on this entity
					if (!attributes.ContainsKey(originalName))
						continue;

					// get attribute
					var attribute = attributes[originalName];
					// change Name-Property
					((IAttributeManagement)attribute).Name = newName;

					// remove from dictionary and add again with new name
					attributes.Remove(originalName);
					attributes.Add(newName, attribute);
				}

				var newEntity = new Entity(sourceEntity, attributes, (sourceEntity.Relationships as RelationshipManager).AllRelationships, sourceEntity.Owner);
				newEntities.Add(newEntity);
			}

			return newEntities;
		}
	}
}