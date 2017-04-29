using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.ImportExport.Models;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Repository.Efc.Parts;
//using ToSic.Eav.Repository.EF4;
//using ToSic.Eav.Repository.EF4.Parts;

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
			var pipelinesAttributeSet = ImpAttrSet.SystemAttributeSet(Constants.DataPipelineStaticName, "Describes a set of data sources and how they are interconnected.",
				new List<ImpAttribute>
				{
					ImpAttribute.StringAttribute("Name", "Pipeline name", "Descriptive Name", true),
					ImpAttribute.StringAttribute("Description", "Description", "Short info about this pipeline, what it's for", true),
					ImpAttribute.BooleanAttribute("AllowEdit", "Allow Edit", "If set to false, the pipeline-system will only show this pipeline but not allow changes.", true, true),
					ImpAttribute.StringAttribute("StreamsOut", "Streams Out", "Comma separated list of streams this pipeline offers to the target. Like 'Content, Presentation, ListContent, ListPresentation'", false),
					ImpAttribute.StringAttribute("StreamWiring", "Stream Wiring", "List of connections between the parts of this pipeline, each connection on one line, like 6730:Default>6732:Default", false, rowCount: 10),
					ImpAttribute.StringAttribute("TestParameters", "Test-Parameters", "Static Parameters to test the Pipeline with. Format as [Token:Property]=Value", true, rowCount: 10)
			});

			var pipelinePartsAttributeSet = ImpAttrSet.SystemAttributeSet(Constants.DataPipelinePartStaticName, "A part in the data pipeline, usually a data source/target element.",
				new List<ImpAttribute>
				{
					ImpAttribute.StringAttribute("Name", "Name", "The part name for easy identification by the user", true),
					ImpAttribute.StringAttribute("Description", "Description", "Notes about this item", true),
					ImpAttribute.StringAttribute("PartAssemblyAndType", "Part Assembly and Type", "Assembly and type info to help the system find this dll etc.", true),
					ImpAttribute.StringAttribute("VisualDesignerData", "Visual Designer Data", "Technical data for the designer so it can save it's values etc.", true),
				});
			#endregion

			#region Define AttributeSets for DataSources Configurations

			var dsrcApp = ImpAttrSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.App", "used to configure an App DataSource", new List<ImpAttribute>());

			var dsrcAttributeFilter = ImpAttrSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.AttributeFilter", "used to configure an AttributeFilter DataSource",
				new List<ImpAttribute>
				{
					ImpAttribute.StringAttribute("AttributeNames", "AttributeNames", null, true),
				});

			var dsrcEntityIdFilter = ImpAttrSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.EntityIdFilter", "used to configure an EntityIdFilter DataSource",
				new List<ImpAttribute>
				{
					ImpAttribute.StringAttribute("EntityIds", "EntityIds", "Comma separated list of Entity IDs, like 503,522,5066,32", true),
					//Import.AttributeHelperTools.BooleanAttribute("PassThroughOnEmptyEntityIds", "Pass-Throught on empty EntityIds", "If this is true and EntityIds results to an empty list, all entities are passed through.", true, false),
				});

			var dsrcEntityTypeFilter = ImpAttrSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.EntityTypeFilter", "used to configure an EntityTypeFilter DataSource",
				new List<ImpAttribute>
				{
					ImpAttribute.StringAttribute("TypeName", "TypeName", null, true),
				});

			var dsrcValueFilter = ImpAttrSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.ValueFilter", "used to configure a ValueFilter DataSource",
				new List<ImpAttribute>
				{
					ImpAttribute.StringAttribute("Attribute", "Attribute", null, true),
					ImpAttribute.StringAttribute("Value", "Value", null, true),
					//Import.AttributeHelperTools.BooleanAttribute("PassThroughOnEmptyValue", "Pass-Throught on empty Value", "If this is true and Value results to an empty string, all entities are passed through.", true, false)
				});

			var dsrcValueSort = ImpAttrSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.ValueSort", "used to configure a ValueSort DataSource",
				new List<ImpAttribute>
				{
					ImpAttribute.StringAttribute("Attributes", "Attributes", null, true),
					ImpAttribute.StringAttribute("Directions", "Directions", null, true),
				});

			var dsrcRelationshipFilter = ImpAttrSet.SystemAttributeSet("|Config ToSic.Eav.DataSources.RelationshipFilter", "used to configure a RelationshipFilter DataSource",
				new List<ImpAttribute>
				{
					ImpAttribute.StringAttribute("Relationship", "Relationship", null, true),
					ImpAttribute.StringAttribute("Filter", "Filter", null, true),
					//Import.AttributeHelperTools.BooleanAttribute("PassThroughOnEmptyFilter", "Pass-Throught on empty Filter", "If this is true and Filter results to an empty string, all entities are passed through.", true, false),
				});

			#endregion

			// Collect AttributeSets for use in Import
			var attributeSets = new List<ImpAttrSet>
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
