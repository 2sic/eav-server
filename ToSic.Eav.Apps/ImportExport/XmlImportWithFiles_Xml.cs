using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence.Logging;

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
            var wrapLog = Log.Call<bool>($"z#{zoneId}, a#{appId}, leaveExisting:{leaveExistingValuesUntouched}");
		    _eavContext = Deps._dbDataForAppImport.Value.Init(zoneId, appId, Log);
            
			AppId = appId;
			ZoneId = zoneId;

			if (!IsCompatible(doc))
			{
				Messages.Add(new Message(Log.AddAndReuse("The import file is not compatible with the installed version of 2sxc."), Message.MessageTypes.Error));
				return false;
			}

			// Get root node "SexyContent"
			var xmlSource = doc.Element(XmlConstants.RootNode);
            if (xmlSource == null)
            {
                Messages.Add(new Message(Log.AddAndReuse("Xml doesn't have expected root node: " + XmlConstants.RootNode), Message.MessageTypes.Error));
                return false;
            }
            PrepareFolderIdCorrectionListAndCreateMissingFolders(xmlSource);
			PrepareFileIdCorrectionList(xmlSource);

            #region Prepare dimensions (languages) based on header...
            var sourceDimensions = BuildSourceDimensionsList(xmlSource);
		    Log.A($"build source dims list⋮{sourceDimensions?.Count}");

            var sourceDefaultLanguage = xmlSource.Element(XmlConstants.Header)?.Element(XmlConstants.Language)?.Attribute(XmlConstants.LangDefault)?.Value;
		    if (sourceDimensions == null || sourceDefaultLanguage == null)
		    {
                Messages.Add(new Message(Log.AddAndReuse("Can't find source dimensions or source-default language."), Message.MessageTypes.Error));
                return false;
            }

            var sourceDefaultDimensionId = sourceDimensions.Any() ?
                sourceDimensions.FirstOrDefault(p => p.Matches(sourceDefaultLanguage))?.DimensionId
				: new int?();

		    Log.A($"source def dim:{sourceDefaultDimensionId}");

            _targetDimensions = Deps.AppStates.Languages(zoneId, true);

            _xmlBuilder = Deps._xmlToEntity.Value.Init(Log).Init(AppId, sourceDimensions, sourceDefaultDimensionId, _targetDimensions, DefaultLanguage);
            #endregion

            var atsNodes = xmlSource.Element(XmlConstants.AttributeSets)?.Elements(XmlConstants.AttributeSet);
		    var entNodes = xmlSource.Elements(XmlConstants.Entities).Elements(XmlConstants.Entity).ToList();

            var importAttributeSets = GetImportContentTypes(atsNodes);
		    var importEntities = BuildEntities(entNodes, (int)TargetTypes.None);


			var import = Deps._importerLazy.Value.Init(ZoneId, AppId, leaveExistingValuesUntouched, true, Log);

			import.ImportIntoDb(importAttributeSets, importEntities.Cast<Entity>().ToList());

            Log.A($"Purging {ZoneId}/{AppId}");
            Deps.SystemManager.Purge(ZoneId, AppId);

			Messages.AddRange(GetExportImportMessagesFromImportLog(import.Storage.ImportLogToBeRefactored));

		    return wrapLog("done", true);
		}


	}

}