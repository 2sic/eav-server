using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using ToSic.Eav.ImportExport;

namespace ToSic.Eav.Testing
{
	class ExportImportTest
	{
		private readonly EavContext _ctx = EavContext.Instance(appId: 2);

		[Test]
		public void EntityExportTest()
		{
			var dsrc = DataSource.GetInitialDataSource(_ctx.AppId);
			var entityModel = dsrc[Constants.DefaultStreamName].List[317];

			var export = new XmlExport(_ctx);
			var entityXElement = export.GetEntityXElement(entityModel);
			Debug.Write(entityXElement);
		}

		[Test]
		public void EntityImportTest()
		{
			// Export an Entity as XML
			var dsrc = DataSource.GetInitialDataSource(_ctx.AppId);
			var entityModel = dsrc[Constants.DefaultStreamName].List[317];
			var export = new XmlExport(_ctx);
			var entityXElement = export.GetEntityXElement(entityModel);

			// Import the Entity from XML
			var xmlImport = new XmlImport();
			//var importEntity = xmlImport.GetImportEntity(entityXElement, entityModel.AssignmentObjectTypeId);


			//// Actually Import the Entity as a new one, so create a new Guid
			//importEntity.EntityGuid = Guid.NewGuid();
			//var import = new Import.Import(_ctx.ZoneId, _ctx.AppId, "EntityImportTest");
			//import.RunImport(null, new List<Import.Entity> {importEntity});
		}
	}
}
