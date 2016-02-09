using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.BLL;
using ToSic.Eav.Persistence;

namespace ToSic.Eav
{
	/// <summary>
	/// Helpers to Upgrade EAV from earlier Versions
	/// </summary>
	public class VersionUpgrade
	{
		private readonly EavDataController _metaDataCtx = EavDataController.Instance(Constants.DefaultZoneId, Constants.MetaDataAppId);
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
			var pipelinesAttributeSet = Import.ImportAttributeSet.SystemAttributeSet(Constants.DataPipelineStaticName, "Describes a set of data sources and how they are interconnected.",
				new List<Import.ImportAttribute>
				{
					Import.ImportAttribute.StringAttribute("Name", "Pipeline name", "Descriptive Name", true),
					Import.ImportAttribute.StringAttribute("Description", "Description", "Short info about this pipeline, what it's for", true),
					Import.ImportAttribute.BooleanAttribute("AllowEdit", "Allow Edit", "If set to false, the pipeline-system will only show this pipeline but not allow changes.", true, true),
					Import.ImportAttribute.StringAttribute("StreamsOut", "Streams Out", "Comma separated list of streams this pipeline offers to the target. Like 'Content, Presentation, ListContent, ListPresentation'", false),
					Import.ImportAttribute.StringAttribute("StreamWiring", "Stream Wiring", "List of connections between the parts of this pipeline, each connection on one line, like 6730:Default>6732:Default", false, rowCount: 10),
					Import.ImportAttribute.StringAttribute("TestParameters", "Test-Parameters", "Static Parameters to test the Pipeline with. Format as [Token:Property]=Value", true, rowCount: 10)
			});

			var pipelinePartsAttributeSet = Import.ImportAttributeSet.SystemAttributeSet(Constants.DataPipelinePartStaticName, "A part in the data pipeline, usually a data source/target element.",
				new List<Import.ImportAttribute>
				{
					Import.ImportAttribute.StringAttribute("Name", "Name", "The part name for easy identification by the user", true),
					Import.ImportAttribute.StringAttribute("Description", "Description", "Notes about this item", true),
					Import.ImportAttribute.StringAttribute("PartAssemblyAndType", "Part Assembly and Type", "Assembly and type info to help the system find this dll etc.", true),
					Import.ImportAttribute.StringAttribute("VisualDesignerData", "Visual Designer Data", "Technical data for the designer so it can save it's values etc.", true),
				});
			#endregion

			#region Define AttributeSets for DataSources Configurations

			var dsrcApp = Import.ImportAttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.App", "used to configure an App DataSource", new List<Import.ImportAttribute>());

			var dsrcAttributeFilter = Import.ImportAttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.AttributeFilter", "used to configure an AttributeFilter DataSource",
				new List<Import.ImportAttribute>
				{
					Import.ImportAttribute.StringAttribute("AttributeNames", "AttributeNames", null, true),
				});

			var dsrcEntityIdFilter = Import.ImportAttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.EntityIdFilter", "used to configure an EntityIdFilter DataSource",
				new List<Import.ImportAttribute>
				{
					Import.ImportAttribute.StringAttribute("EntityIds", "EntityIds", "Comma separated list of Entity IDs, like 503,522,5066,32", true),
					//Import.AttributeHelperTools.BooleanAttribute("PassThroughOnEmptyEntityIds", "Pass-Throught on empty EntityIds", "If this is true and EntityIds results to an empty list, all entities are passed through.", true, false),
				});

			var dsrcEntityTypeFilter = Import.ImportAttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.EntityTypeFilter", "used to configure an EntityTypeFilter DataSource",
				new List<Import.ImportAttribute>
				{
					Import.ImportAttribute.StringAttribute("TypeName", "TypeName", null, true),
				});

			var dsrcValueFilter = Import.ImportAttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.ValueFilter", "used to configure a ValueFilter DataSource",
				new List<Import.ImportAttribute>
				{
					Import.ImportAttribute.StringAttribute("Attribute", "Attribute", null, true),
					Import.ImportAttribute.StringAttribute("Value", "Value", null, true),
					//Import.AttributeHelperTools.BooleanAttribute("PassThroughOnEmptyValue", "Pass-Throught on empty Value", "If this is true and Value results to an empty string, all entities are passed through.", true, false)
				});

			var dsrcValueSort = Import.ImportAttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.ValueSort", "used to configure a ValueSort DataSource",
				new List<Import.ImportAttribute>
				{
					Import.ImportAttribute.StringAttribute("Attributes", "Attributes", null, true),
					Import.ImportAttribute.StringAttribute("Directions", "Directions", null, true),
				});

			var dsrcRelationshipFilter = Import.ImportAttributeSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.RelationshipFilter", "used to configure a RelationshipFilter DataSource",
				new List<Import.ImportAttribute>
				{
					Import.ImportAttribute.StringAttribute("Relationship", "Relationship", null, true),
					Import.ImportAttribute.StringAttribute("Filter", "Filter", null, true),
					//Import.AttributeHelperTools.BooleanAttribute("PassThroughOnEmptyFilter", "Pass-Throught on empty Filter", "If this is true and Filter results to an empty string, all entities are passed through.", true, false),
				});

			#endregion

			// Collect AttributeSets for use in Import
			var attributeSets = new List<Import.ImportAttributeSet>
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
		    var x = cache.LightList.First();

            var import = new Import.Import(Constants.DefaultZoneId, Constants.MetaDataAppId, _userName);
			import.RunImport(attributeSets, null);

			#region Mark all AttributeSets as shared and ensure they exist on all Apps
			foreach (var attributeSet in attributeSets)
				_metaDataCtx.AttribSet.GetAttributeSet(attributeSet.StaticName).AlwaysShareConfiguration = true;

			_metaDataCtx.SqlDb.SaveChanges();

			_metaDataCtx.AttribSet.EnsureSharedAttributeSets();
			#endregion
		}
	}
}
