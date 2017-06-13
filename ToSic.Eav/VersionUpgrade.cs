using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Repository.Efc.Parts;
using AttributeDefinition = ToSic.Eav.Data.Builder.AttributeDefinition;
using ContentType = ToSic.Eav.Data.Builder.ContentType;

namespace ToSic.Eav
{
	/// <summary>
	/// Helpers to Upgrade EAV from earlier Versions
	/// </summary>
	public class VersionUpgrade
	{
		private readonly DbDataController _metaDataCtx = DbDataController.Instance(Constants.DefaultZoneId, Constants.MetaDataAppId);
		//private readonly DbDataController _metaDataCtx = DbDataController.Instance(Constants.DefaultZoneId, Constants.MetaDataAppId);
		private readonly string _userName;

		/// <summary>
		/// Constructor
		/// </summary>
		public VersionUpgrade(string userName)
		{
			_userName = userName;
		}

		/// <summary>
		/// Create Pipeline Designer Entities if they don't exist yet. Uses the EAV Import API.
		/// </summary>
		public void EnsurePipelineDesignerAttributeSets()
		{
			#region Define AttributeSets for DataPipeline and DataPipelinePart
			var pipelinesAttributeSet = ContentType.SystemAttributeSet(Constants.DataPipelineStaticName, "Describes a set of data sources and how they are interconnected.",
				new List<IAttributeDefinition>
				{
                    AttributeDefinition.StringAttribute("Name", "Pipeline name", "Descriptive Name", true),
                    AttributeDefinition.StringAttribute("Description", "Description", "Short info about this pipeline, what it's for", true),
                    AttributeDefinition.BooleanAttribute("AllowEdit", "Allow Edit", "If set to false, the pipeline-system will only show this pipeline but not allow changes.", true, true),
                    AttributeDefinition.StringAttribute("StreamsOut", "Streams Out", "Comma separated list of streams this pipeline offers to the target. Like 'Content, Presentation, ListContent, ListPresentation'", false),
                    AttributeDefinition.StringAttribute("StreamWiring", "Stream Wiring", "List of connections between the parts of this pipeline, each connection on one line, like 6730:Default>6732:Default", false, rowCount: 10),
                    AttributeDefinition.StringAttribute("TestParameters", "Test-Parameters", "Static Parameters to test the Pipeline with. Format as [Token:Property]=Value", true, rowCount: 10)
			});

			var pipelinePartsAttributeSet = ContentType.SystemAttributeSet(Constants.DataPipelinePartStaticName, "A part in the data pipeline, usually a data source/target element.",
				new List<IAttributeDefinition>
				{
                    AttributeDefinition.StringAttribute("Name", "Name", "The part name for easy identification by the user", true),
                    AttributeDefinition.StringAttribute("Description", "Description", "Notes about this item", true),
                    AttributeDefinition.StringAttribute("PartAssemblyAndType", "Part Assembly and Type", "Assembly and type info to help the system find this dll etc.", true),
                    AttributeDefinition.StringAttribute("VisualDesignerData", "Visual Designer Data", "Technical data for the designer so it can save it's values etc.", true),
				});
			#endregion

			#region Define AttributeSets for DataSources Configurations

			var dsrcApp = ContentType.SystemAttributeSet("|Config ToSic.Eav.DataSources.App", "used to configure an App DataSource", new List<IAttributeDefinition>());

			var dsrcAttributeFilter = ContentType.SystemAttributeSet("|Config ToSic.Eav.DataSources.AttributeFilter", "used to configure an AttributeFilter DataSource",
				new List<IAttributeDefinition>
				{
                    AttributeDefinition.StringAttribute("AttributeNames", "AttributeNames", null, true),
				});

			var dsrcEntityIdFilter = ContentType.SystemAttributeSet("|Config ToSic.Eav.DataSources.EntityIdFilter", "used to configure an EntityIdFilter DataSource",
				new List<IAttributeDefinition>
				{
                    AttributeDefinition.StringAttribute("EntityIds", "EntityIds", "Comma separated list of Entity IDs, like 503,522,5066,32", true),
					//Import.AttributeHelperTools.BooleanAttribute("PassThroughOnEmptyEntityIds", "Pass-Throught on empty EntityIds", "If this is true and EntityIds results to an empty list, all entities are passed through.", true, false),
				});

			var dsrcEntityTypeFilter = ContentType.SystemAttributeSet("|Config ToSic.Eav.DataSources.EntityTypeFilter", "used to configure an EntityTypeFilter DataSource",
				new List<IAttributeDefinition>
				{
                    AttributeDefinition.StringAttribute("TypeName", "TypeName", null, true),
				});

			var dsrcValueFilter = ContentType.SystemAttributeSet("|Config ToSic.Eav.DataSources.ValueFilter", "used to configure a ValueFilter DataSource",
				new List<IAttributeDefinition>
				{
                    AttributeDefinition.StringAttribute("Attribute", "Attribute", null, true),
                    AttributeDefinition.StringAttribute("Value", "Value", null, true),
					//Import.AttributeHelperTools.BooleanAttribute("PassThroughOnEmptyValue", "Pass-Throught on empty Value", "If this is true and Value results to an empty string, all entities are passed through.", true, false)
				});

			var dsrcValueSort = ContentType.SystemAttributeSet("|Config ToSic.Eav.DataSources.ValueSort", "used to configure a ValueSort DataSource",
				new List<IAttributeDefinition>
				{
                    AttributeDefinition.StringAttribute("Attributes", "Attributes", null, true),
                    AttributeDefinition.StringAttribute("Directions", "Directions", null, true),
				});

			var dsrcRelationshipFilter = ContentType.SystemAttributeSet("|Config ToSic.Eav.DataSources.RelationshipFilter", "used to configure a RelationshipFilter DataSource",
				new List<IAttributeDefinition>
				{
                    AttributeDefinition.StringAttribute("Relationship", "Relationship", null, true),
                    AttributeDefinition.StringAttribute("Filter", "Filter", null, true),
					//Import.AttributeHelperTools.BooleanAttribute("PassThroughOnEmptyFilter", "Pass-Throught on empty Filter", "If this is true and Filter results to an empty string, all entities are passed through.", true, false),
				});

			#endregion

			// Collect AttributeSets for use in Import
			var attributeSets = new List<Data.ContentType>
			{
				pipelinesAttributeSet,
				pipelinePartsAttributeSet,
				dsrcApp,
				dsrcAttributeFilter,
				dsrcEntityIdFilter,
				dsrcEntityTypeFilter,
				dsrcValueFilter,
				dsrcValueSort,
				dsrcRelationshipFilter
			};

		    // try to access cache before we start the import, to ensure it's available afterwards (very, very important!)
		    var cache = DataSource.GetCache(Constants.DefaultZoneId, Constants.MetaDataAppId);
		    // ReSharper disable once UnusedVariable
		    var x = cache.LightList.First();

            var import = new DbImport(Constants.DefaultZoneId, Constants.MetaDataAppId/*, _userName*/);
			import.ImportIntoDb(attributeSets, null);

			#region Mark all AttributeSets as shared and ensure they exist on all Apps
			foreach (var attributeSet in attributeSets)
				_metaDataCtx.AttribSet.GetAttributeSet(attributeSet.StaticName).AlwaysShareConfiguration = true;

			_metaDataCtx.SqlDb.SaveChanges();

			_metaDataCtx.AttribSet.EnsureSharedAttributeSetsOnEverythingAndSave();
			#endregion
		}
	}
}
