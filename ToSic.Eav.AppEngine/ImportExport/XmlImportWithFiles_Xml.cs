using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Repository.Efc;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.Apps.ImportExport
{
    public partial class XmlImportWithFiles
    {
		/// <summary>
		/// Do the import
		/// </summary>
		public bool ImportXml(int zoneId, int appId, XDocument doc, bool leaveExistingValuesUntouched = true)
		{
		    Log.Add($"import xml z#{zoneId}, a#{appId}, leaveExisting:{leaveExistingValuesUntouched}");
		    _eavContext = DbDataController.Instance(zoneId, appId, Log);
            
			AppId = appId;
			ZoneId = zoneId;

			if (!IsCompatible(doc))
			{
				Messages.Add(new Message("The import file is not compatible with the installed version of 2sxc.", Message.MessageTypes.Error));
				return false;
			}

			// Get root node "SexyContent"
			var xmlSource = doc.Element(XmlConstants.RootNode);
            if (xmlSource == null)
            {
                Messages.Add(new Message("Xml doesn't have expected root node: " + XmlConstants.RootNode, Message.MessageTypes.Error));
                return false;
            }
            PrepareFolderIdCorrectionListAndCreateMissingFolders(xmlSource);
			PrepareFileIdCorrectionList(xmlSource);

            #region Prepare dimensions (languages) based on header...
            var sourceDimensions = BuildSourceDimensionsList(xmlSource);
		    Log.Add($"build source dims list⋮{sourceDimensions?.Count}");

            var sourceDefaultLanguage = xmlSource.Element(XmlConstants.Header)?.Element(XmlConstants.Language)?.Attribute(XmlConstants.LangDefault)?.Value;
		    if (sourceDimensions == null || sourceDefaultLanguage == null)
		    {
                Messages.Add(new Message("Cant find source dimensions or source-default language.", Message.MessageTypes.Error));
                return false;
            }

            var sourceDefaultDimensionId = sourceDimensions.Any() ?
                sourceDimensions.FirstOrDefault(p => p.Matches(sourceDefaultLanguage))?.DimensionId
				: new int?();

		    Log.Add($"source def dim:{sourceDefaultDimensionId}");

            _targetDimensions = new ZoneRuntime(zoneId, Log).Languages(true);

            _xmlBuilder = new XmlToEntity(AppId, sourceDimensions, sourceDefaultDimensionId, _targetDimensions, DefaultLanguage, Log);
            #endregion

            var atsNodes = xmlSource.Element(XmlConstants.AttributeSets)?.Elements(XmlConstants.AttributeSet);
		    var entNodes = xmlSource.Elements(XmlConstants.Entities).Elements(XmlConstants.Entity).ToList();

            var importAttributeSets = GetImportContentTypes(atsNodes);
		    var importEntities = BuildEntities(entNodes, Constants.NotMetadata);


			var import = new Import(ZoneId, AppId, leaveExistingValuesUntouched, parentLog: Log);
			import.ImportIntoDb(importAttributeSets, importEntities.Cast<Entity>());
            SystemManager.Purge(ZoneId, AppId);

			Messages.AddRange(GetExportImportMessagesFromImportLog(import.Storage.ImportLogToBeRefactored));

			//if (xmlSource.Elements(XmlConstants.Templates).Any())
			//	ImportXmlTemplates(xmlSource);

		    Log.Add("import xml completed");
			return true;
		}


	}

}