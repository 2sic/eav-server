using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Interfaces;
using ToSic.Eav.ImportExport.Logging;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Repository.Efc.Parts;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.Apps.ImportExport
{
    public class XmlImportWithFiles
	{
		public List<ExportImportMessage> ImportLog;

		//private List<ToSicEavDimensions> _sourceDimensions;
		private List<Dimension> _sourceDimensions;
		private string _sourceDefaultLanguage;
		private int? _sourceDefaultDimensionId;
		//private List<ToSicEavDimensions> _targetDimensions;
        private List<Dimension> _targetDimensions;
        private DbDataController _eavContext;
		private int _appId;
		private int _zoneId;
		private readonly Dictionary<int, int> _fileIdCorrectionList = new Dictionary<int, int>();
	    private readonly Dictionary<int, int> _folderIdCorrectionList = new Dictionary<int, int>();

        private readonly IImportExportEnvironment _environment;

        /// <summary>
        /// The default language / culture - example: de-DE
        /// </summary>
        private string DefaultLanguage { get; }

        private bool AllowSystemChanges { get; }

		#region Prerequisites

		/// <summary>
		/// Create a new xmlImport instance
		/// </summary>
		/// <param name="defaultLanguage">The portals default language / culture - example: de-DE</param>
		/// <param name="allowSystemChanges">Specify if the import should be able to change system-wide things like shared attributesets</param>
		public XmlImportWithFiles(string defaultLanguage = null, bool allowSystemChanges = false)
		{
		    _environment = Factory.Resolve<IImportExportEnvironment>();
			// Prepare
			ImportLog = new List<ExportImportMessage>();
		    DefaultLanguage = defaultLanguage ?? _environment.DefaultLanguage;
			AllowSystemChanges = allowSystemChanges;
        }

		public bool IsCompatible(XDocument doc)
		{
		    var rns = doc.Elements(XmlConstants.RootNode);
		    var rn = doc.Element(XmlConstants.RootNode);
			// Return if no Root Node "SexyContent"
			if (!rns.Any() || rn == null)
			{
				ImportLog.Add(new ExportImportMessage("The XML file you specified does not seem to be a 2sxc Export.", ExportImportMessage.MessageTypes.Error));
				return false;
			}
			// Return if Version does not match
			if (rn.Attributes().All(a => a.Name != XmlConstants.MinEnvVersion) || new Version(rn.Attribute(XmlConstants.MinEnvVersion).Value) > new Version(_environment.ModuleVersion))
			{
				ImportLog.Add(new ExportImportMessage("This template or app requires version " + rn.Attribute(XmlConstants.MinEnvVersion).Value + " in order to work, you have version " + _environment.ModuleVersion + " installed.", ExportImportMessage.MessageTypes.Error));
				return false;
			}

			return true;
		}

		private void PrepareFileIdCorrectionList(XElement sexyContentNode)
		{
			if (!sexyContentNode.Elements(XmlConstants.PortalFiles).Any())
				return;

			var portalFiles = sexyContentNode.Element(XmlConstants.PortalFiles)?.Elements(XmlConstants.FileNode);
		    if (portalFiles == null) return;
		    var filesAndPaths = portalFiles.ToDictionary(
		        p => int.Parse(p.Attribute(XmlConstants.FileIdAttr).Value),
		        v => v.Attribute(XmlConstants.FolderNodePath/*"RelativePath"*/).Value
		    );
            _environment.MapExistingFilesToImportSet(filesAndPaths, _fileIdCorrectionList);

		}


	    private void PrepareFolderIdCorrectionListAndCreateMissingFolders(XElement sexyContentNode)
        {
            if (!sexyContentNode.Elements(XmlConstants.FolderGroup).Any()) 
                return;

            var portalFiles = sexyContentNode.Element(XmlConstants.FolderGroup)?.Elements(XmlConstants.Folder);
            if (portalFiles == null) return;

            var foldersAndPath = portalFiles.ToDictionary(
                p => int.Parse(p.Attribute(XmlConstants.FolderNodeId).Value),
                v => v.Attribute(XmlConstants.FolderNodePath).Value
            );
            _environment.CreateFoldersAndMapToImportIds(foldersAndPath, _folderIdCorrectionList, ImportLog);
        }
	    #endregion

        /// <summary>
        /// Creates an app and then imports the xml
        /// </summary>
        /// <returns>AppId of the new imported app</returns>
        public bool ImportApp(int zoneId, XDocument doc, out int? appId)
		{
			// Increase script timeout to prevent timeouts
			//HttpContext.Current.Server.ScriptTimeout = 300;

			appId = new int?();

			if (!IsCompatible(doc))
			{
				ImportLog.Add(new ExportImportMessage("The import file is not compatible with the installed version of 2sxc.", ExportImportMessage.MessageTypes.Error));
				return false;
			}

			// Get root node "SexyContent"
			var xmlSource = doc.Element(XmlConstants.RootNode);
			var xApp = xmlSource?.Element(XmlConstants.Header)?.Element(XmlConstants.App);

			var appGuid = xApp?.Attribute(XmlConstants.Guid)?.Value;

            if (appGuid == null)
            {
                ImportLog.Add(new ExportImportMessage("Something is wrong in the xml structure, can't get an app-guid", ExportImportMessage.MessageTypes.Error));
                return false;
            }

            if (appGuid != XmlConstants.AppContentGuid)
			{
				// Build Guid (take existing, or create a new)
				if (String.IsNullOrEmpty(appGuid) || appGuid == new Guid().ToString())
					appGuid = Guid.NewGuid().ToString();

				// Adding app to EAV
                var eavDc = DbDataController.Instance(zoneId);
			    var app = eavDc.App.AddApp(null, appGuid);
				eavDc.SqlDb.SaveChanges();

				appId = app.AppId;
			}
			else
			{
				appId = _appId;
			}

			if (appId <= 0)
			{
				ImportLog.Add(new ExportImportMessage("App was not created. Please try again or make sure the package you are importing is correct.", ExportImportMessage.MessageTypes.Error));
				return false;
			}

            DataSource.GetCache(null).PurgeGlobalCache();   // must do this, to ensure that the app-id exists now 
			return ImportXml(zoneId, appId.Value, doc);
		}

		/// <summary>
		/// Do the import
		/// </summary>
		public bool ImportXml(int zoneId, int appId, XDocument doc, bool leaveExistingValuesUntouched = true)
		{
		    _eavContext = DbDataController.Instance(zoneId, appId);
            
			_appId = appId;
			_zoneId = zoneId;

			if (!IsCompatible(doc))
			{
				ImportLog.Add(new ExportImportMessage("The import file is not compatible with the installed version of 2sxc.", ExportImportMessage.MessageTypes.Error));
				return false;
			}

			// Get root node "SexyContent"
			var xmlSource = doc.Element(XmlConstants.RootNode);
            if (xmlSource == null)
            {
                ImportLog.Add(new ExportImportMessage("Xml doesn't have expected root node: " + XmlConstants.RootNode, ExportImportMessage.MessageTypes.Error));
                return false;
            }
            PrepareFolderIdCorrectionListAndCreateMissingFolders(xmlSource);
			PrepareFileIdCorrectionList(xmlSource);

            #region Prepare dimensions (languages) based on header...
            var sDimensions = BuildSourceDimensionsList(xmlSource);

			_sourceDefaultLanguage = xmlSource.Element(XmlConstants.Header)?.Element(XmlConstants.Language)?.Attribute(XmlConstants.LangDefault)?.Value;
		    if (sDimensions == null || _sourceDefaultLanguage == null)
		    {
                ImportLog.Add(new ExportImportMessage("Cant find source dimensions or source-default language.", ExportImportMessage.MessageTypes.Error));
                return false;
            }

            _sourceDefaultDimensionId = sDimensions.Any() ?
                sDimensions.FirstOrDefault(p => p.ExternalKey == _sourceDefaultLanguage)?.DimensionId
				: new int?();
            _sourceDimensions = sDimensions.Select(s => new Dimension { DimensionId = s.DimensionId, Key = s.ExternalKey }).ToList();

            // 2017-06-13 2dm moving to dimensions layer...
            //var langs = _eavContext.Dimensions.GetLanguages();//.GetDimensionChildren(Constants.CultureSystemKey);
            //if (langs.Count == 0)
            //	langs.Add(new ToSicEavDimensions
            //	{
            //		Active = true,
            //		ExternalKey = DefaultLanguage,
            //		Name = "(added by import System, default language " + DefaultLanguage + ")",
            //		SystemKey = Constants.CultureSystemKey
            //	});
            //_targetDimensions = langs.Select(d => new Data.Dimension { DimensionId = d.DimensionId, Key = d.ExternalKey }).ToList();
            _targetDimensions = _eavContext.Dimensions.GetLanguageListForImport(DefaultLanguage);
            #endregion

            var atsNodes = xmlSource.Element(XmlConstants.AttributeSets)?.Elements(XmlConstants.AttributeSet);
		    var entNodes = xmlSource.Elements(XmlConstants.Entities).Elements(XmlConstants.Entity);

            var importAttributeSets = GetImportAttributeSets(atsNodes);
		    var importEntities = GetImportEntities(entNodes, Constants.NotMetadata);


			var import = new DbImport(_zoneId, _appId, leaveExistingValuesUntouched);
			import.ImportIntoDb(importAttributeSets, importEntities);
            SystemManager.Purge(_zoneId, _appId);

			ImportLog.AddRange(GetExportImportMessagesFromImportLog(import.ImportLog));

			if (xmlSource.Elements(XmlConstants.Templates).Any())
				ImportXmlTemplates(xmlSource);

			return true;
		}

	    private static List<ToSicEavDimensions> BuildSourceDimensionsList(XElement xmlSource)
	    {
	        var sDimensions =
	            xmlSource.Element(XmlConstants.Header)?
	                .Element(XmlConstants.DimensionDefinition)?
	                .Elements(XmlConstants.DimensionDefElement)
	                .Select(p => new ToSicEavDimensions
	                {
	                    DimensionId = int.Parse(p.Attribute(XmlConstants.DimId).Value),
	                    Name = p.Attribute(XmlConstants.Name).Value,
	                    SystemKey = p.Attribute(XmlConstants.CultureSysKey).Value,
	                    ExternalKey = p.Attribute(XmlConstants.CultureExtKey).Value,
	                    Active = Boolean.Parse(p.Attribute(XmlConstants.CultureIsActiveAttrib).Value)
	                }).ToList();
	        return sDimensions;
	    }

	    /// <summary>
        /// Maps EAV import messages to 2sxc import messages
        /// </summary>
        /// <param name="importLog"></param>
        /// <returns></returns>
        private IEnumerable<ExportImportMessage> GetExportImportMessagesFromImportLog(List<ImportLogItem> importLog)
            => importLog.Select(l => new ExportImportMessage(l.Message,
                l.EntryType == EventLogEntryType.Error
                    ? ExportImportMessage.MessageTypes.Error
                    : l.EntryType == EventLogEntryType.Information
                        ? ExportImportMessage.MessageTypes.Information
                        : ExportImportMessage.MessageTypes.Warning
                ));
		

		#region AttributeSets

		private List<ContentType> GetImportAttributeSets(IEnumerable<XElement> xAttributeSets)
		{
            var importAttributeSets = new List<ContentType>();

			// Loop through AttributeSets
			foreach (var attributeSet in xAttributeSets)
			{
				var attributes = new List<IAttributeDefinition>();
                //ImpAttribDefinition titleAttribute = null;// = new ImpAttribute();
			    var attsetElem = attributeSet.Element(XmlConstants.Attributes);
                if (attsetElem != null)
                    foreach (var xElementAttribute in attsetElem.Elements(XmlConstants.Attribute))
                    {
                        var attribute = new AttributeDefinition(
                            xElementAttribute.Attribute(XmlConstants.Static).Value,
                            null,
                            xElementAttribute.Attribute(XmlConstants.EntityTypeAttribute).Value,
                            null, null, null
                        );
                        attribute.InternalAttributeMetaData = GetImportEntities(xElementAttribute.Elements(XmlConstants.Entity), Constants.MetadataForField);
                        attributes.Add(attribute);

                        // Set Title Attribute
                        if (Boolean.Parse(xElementAttribute.Attribute(XmlConstants.IsTitle).Value))
                        {
                            attribute.IsTitle = true;
                            //titleAttribute = attribute;
                        }
                    }
                // check if it's normal (not a ghost) but still missing a title
			    if(attributes.Any() && !attributes.Any(a => a.IsTitle)) 
			        (attributes.First() as AttributeDefinition).IsTitle = true;

			    // Add AttributeSet
                var ct = new ContentType(attributeSet.Attribute(XmlConstants.Name).Value)
				{
					Attributes = attributes,
                    OnSaveUseParentStaticName = attributeSet.Attributes(XmlConstants.AttributeSetParentDef).Any() ? attributeSet.Attribute(XmlConstants.AttributeSetParentDef).Value : "",
                    OnSaveSortAttributes = attributeSet.Attributes(XmlConstants.SortAttributes).Any() && bool.Parse(attributeSet.Attribute(XmlConstants.SortAttributes).Value)
				};
			    ct.SetImportParameters(
			        scope: attributeSet.Attributes(XmlConstants.Scope).Any()
			            ? attributeSet.Attribute(XmlConstants.Scope).Value
			            : _environment.FallbackContentTypeScope,
                    staticName:attributeSet.Attribute(XmlConstants.Static).Value,
                    description: attributeSet.Attribute(XmlConstants.Description).Value,
                    alwaysShareDef: AllowSystemChanges && attributeSet.Attributes(XmlConstants.AlwaysShareConfig).Any() &&
			                        Boolean.Parse(attributeSet.Attribute(XmlConstants.AlwaysShareConfig).Value)
			    );
			    importAttributeSets.Add(ct);

			}

			return importAttributeSets;
		}

		#endregion

		#region Templates

        private void ImportXmlTemplates(XElement root)
        {
            var templates = root.Element(XmlConstants.Templates);
            if (templates == null) return;

            var cache = DataSource.GetCache(_zoneId, _appId);

            foreach (var template in templates.Elements(XmlConstants.Template))
            {
                var name = "";
                try
                {
                    name = template.Attribute(XmlConstants.Name).Value;
                    var path = template.Attribute(AppConstants.TemplatePath).Value;

                    var contentTypeStaticName = template.Attribute(XmlConstants.AttSetStatic).Value;

                    if (!String.IsNullOrEmpty(contentTypeStaticName) && cache.GetContentType(contentTypeStaticName) == null)
                    {
                        ImportLog.Add(new ExportImportMessage($"Content Type for Template \'{name}\' could not be found. The template has not been imported.",
                                ExportImportMessage.MessageTypes.Warning));
                        continue;
                    }

                    var demoEntityGuid = template.Attribute(XmlConstants.TemplateDemoItemGuid).Value;
                    var demoEntityId = new int?();

                    if (!String.IsNullOrEmpty(demoEntityGuid))
                    {
                        var entityGuid = Guid.Parse(demoEntityGuid);
                        if (_eavContext.Entities.EntityExists(entityGuid))
                            demoEntityId = _eavContext.Entities.GetMostCurrentDbEntity(entityGuid).EntityId;
                        else
                            ImportLog.Add(new ExportImportMessage($"Demo Entity for Template \'{name}\' could not be found. (Guid: {demoEntityGuid})", ExportImportMessage.MessageTypes.Information));

                    }

                    var type = template.Attribute(XmlConstants.EntityTypeAttribute).Value;
                    var isHidden = Boolean.Parse(template.Attribute(AppConstants.TemplateIsHidden).Value);
                    var location = template.Attribute(AppConstants.TemplateLocation).Value;
                    var publishData =
                        Boolean.Parse(template.Attribute(AppConstants.TemplatePublishEnable) == null
                            ? "False"
                            : template.Attribute(AppConstants.TemplatePublishEnable).Value);
                    var streamsToPublish = template.Attribute(AppConstants.TemplatePublishStreams) == null
                        ? ""
                        : template.Attribute(AppConstants.TemplatePublishStreams).Value;
                    var viewNameInUrl = template.Attribute(AppConstants.TemplateViewName) == null
                        ? null
                        : template.Attribute(AppConstants.TemplateViewName).Value;

                    var pipelineEntityGuid = template.Attribute(XmlConstants.TemplatePipelineGuid);
                    var pipelineEntityId = new int?();

                    if (!String.IsNullOrEmpty(pipelineEntityGuid?.Value))
                    {
                        var entityGuid = Guid.Parse(pipelineEntityGuid.Value);
                        if (_eavContext.Entities.EntityExists(entityGuid))
                            pipelineEntityId = _eavContext.Entities.GetMostCurrentDbEntity(entityGuid).EntityId;
                        else
                            ImportLog.Add(new ExportImportMessage($"Pipeline Entity for Template \'{name}\' could not be found. (Guid: {pipelineEntityGuid.Value})", ExportImportMessage.MessageTypes.Information));
                    }

                    var useForList = false;
                    if (template.Attribute(AppConstants.TemplateUseList) != null)
                        useForList = Boolean.Parse(template.Attribute(AppConstants.TemplateUseList).Value);

                    var lstTemplateDefaults = template.Elements(XmlConstants.Entity).Select(e =>
                    {
                        var xmlItemType =
                            e.Elements(XmlConstants.ValueNode)
                                .FirstOrDefault(v => v.Attribute(XmlConstants.KeyAttr).Value == XmlConstants.TemplateItemType)?
                                .Attribute(XmlConstants.ValueAttr)
                                .Value;
                        var xmlContentTypeStaticName =
                            e.Elements(XmlConstants.ValueNode)
                                .FirstOrDefault(v => v.Attribute(XmlConstants.KeyAttr).Value == XmlConstants.TemplateContentTypeId)?
                                .Attribute(XmlConstants.ValueAttr)
                                .Value;
                        var xmlDemoEntityGuidString =
                            e.Elements(XmlConstants.ValueNode)
                                .FirstOrDefault(v => v.Attribute(XmlConstants.KeyAttr).Value == XmlConstants.TemplateDemoItemId)?
                                .Attribute(XmlConstants.ValueAttr)
                                .Value;
                        if (xmlItemType == null || xmlContentTypeStaticName == null || xmlDemoEntityGuidString == null)
                        {
                            ImportLog.Add(new ExportImportMessage(
                                $"trouble with template '{name}' - either type, static or guid are null", ExportImportMessage.MessageTypes.Error));
                            return null;
                        }
                        var xmlDemoEntityId = new int?();
                        if (xmlDemoEntityGuidString != "0" && xmlDemoEntityGuidString != "")
                        {
                            var xmlDemoEntityGuid = Guid.Parse(xmlDemoEntityGuidString);
                            if (_eavContext.Entities.EntityExists(xmlDemoEntityGuid))
                                xmlDemoEntityId = _eavContext.Entities.GetMostCurrentDbEntity(xmlDemoEntityGuid).EntityId;
                        }

                        return new TemplateDefault
                        {
                            ItemType = xmlItemType,
                            ContentTypeStaticName =
                                xmlContentTypeStaticName == "0" || xmlContentTypeStaticName == ""
                                    ? ""
                                    : xmlContentTypeStaticName,
                            DemoEntityId = xmlDemoEntityId
                        };
                    }).ToList();

                    // note: Array lstTemplateDefaults has null objects, so remove null objects
                    var templateDefaults = lstTemplateDefaults.Where(lstItem => lstItem != null).ToList();

                    var presentationTypeStaticName = "";
                    var presentationDemoEntityId = new int?();
                    //if list templateDefaults would have null objects, we would have an exception
                    var presentationDefault = templateDefaults.FirstOrDefault(t => t.ItemType == Constants.PresentationKey);
                    if (presentationDefault != null)
                    {
                        presentationTypeStaticName = presentationDefault.ContentTypeStaticName;
                        presentationDemoEntityId = presentationDefault.DemoEntityId;
                    }

                    var listContentTypeStaticName = "";
                    var listContentDemoEntityId = new int?();
                    var listContentDefault = templateDefaults.FirstOrDefault(t => t.ItemType == AppConstants.ListContent);
                    if (listContentDefault != null)
                    {
                        listContentTypeStaticName = listContentDefault.ContentTypeStaticName;
                        listContentDemoEntityId = listContentDefault.DemoEntityId;
                    }

                    var listPresentationTypeStaticName = "";
                    var listPresentationDemoEntityId = new int?();
                    var listPresentationDefault = templateDefaults.FirstOrDefault(t => t.ItemType == AppConstants.ListPresentation);
                    if (listPresentationDefault != null)
                    {
                        listPresentationTypeStaticName = listPresentationDefault.ContentTypeStaticName;
                        listPresentationDemoEntityId = listPresentationDefault.DemoEntityId;
                    }

                    new AppManager(_eavContext.ZoneId, _eavContext.AppId).Templates.CreateOrUpdate(
                        null, name, path, contentTypeStaticName, demoEntityId, presentationTypeStaticName,
                        presentationDemoEntityId, listContentTypeStaticName, listContentDemoEntityId,
                        listPresentationTypeStaticName, listPresentationDemoEntityId, type, isHidden, location,
                        useForList, publishData, streamsToPublish, pipelineEntityId, viewNameInUrl);

                    ImportLog.Add(new ExportImportMessage($"Template \'{name}\' successfully imported.",
                        ExportImportMessage.MessageTypes.Information));
                }

                catch (Exception)
                {
                    ImportLog.Add(new ExportImportMessage($"Import for template \'{name}\' failed.",
                        ExportImportMessage.MessageTypes.Information));
                }

            }
        }

        #endregion

		#region Entities

        /// <summary>
        /// Returns a collection of EAV import entities
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="assignmentObjectTypeId"></param>
        /// <returns></returns>
        private List<Entity> GetImportEntities(IEnumerable<XElement> entities, int assignmentObjectTypeId)
            => entities.Select(e => GetImportEntity(e, assignmentObjectTypeId)).ToList();
		


        /// <summary>
        /// Returns an EAV import entity
        /// </summary>
        /// <param name="entityNode">The xml-Element of the entity to import</param>
        /// <param name="assignmentObjectTypeId">assignmentObjectTypeId</param>
        /// <returns></returns>
        private Entity GetImportEntity(XElement entityNode, int assignmentObjectTypeId)
		{
            #region retrieve optional metadata keys in the import - must happen before we apply corrections like AppId
            Guid? keyGuid = null;
		    var maybeGuid = entityNode.Attribute(XmlConstants.KeyGuid);
            if (maybeGuid != null)
                keyGuid = Guid.Parse(maybeGuid.Value);
            int? keyNumber = null;
		    var maybeNumber = entityNode.Attribute(XmlConstants.KeyNumber);
            if (maybeNumber != null)
                keyNumber = int.Parse(maybeNumber.Value);

            var keyString = entityNode.Attribute(XmlConstants.KeyString)?.Value;
            #endregion

            #region check if the xml has an own assignment object type (then we wouldn't use the default)
            switch (entityNode.Attribute(XmlConstants.KeyTargetType)?.Value)
			{
				// Special case: App AttributeSets must be assigned to the current app
				case XmlConstants.App:
					keyNumber = _appId;
					assignmentObjectTypeId = SystemRuntime.GetKeyTypeId(Constants.AppAssignmentName);// ContentTypeHelpers.AssignmentObjectTypeIDSexyContentApp;
					break;
                case XmlConstants.Entity:
					assignmentObjectTypeId = Constants.MetadataForEntity;
					break;
                case XmlConstants.ContentType:
			        assignmentObjectTypeId = Constants.MetadataForContentType;
                    break;
                case XmlConstants.CmsObject:
			        assignmentObjectTypeId = Constants.MetadataForContentType;

                    if(keyString == null)
                        throw new Exception("found cms object, but couldn't find metadata-key of type string, will abort");
			        var newKey = GetMappedLink(keyString);
			        if (newKey != null)
			            keyString = newKey;
			        break;
			}
            #endregion


            // Special case #2: Corrent values of Template-Describing entities, and resolve files

            foreach (var sourceValue in entityNode.Elements(XmlConstants.ValueNode))
			{
				var sourceValueString = sourceValue.Attribute(XmlConstants.ValueAttr).Value;

				// Correct FileId in Hyperlink fields (takes XML data that lists files)
			    if (!String.IsNullOrEmpty(sourceValueString) && sourceValue.Attribute(XmlConstants.EntityTypeAttribute).Value == XmlConstants.ValueTypeLink)
			    {
			        string newValue = GetMappedLink(sourceValueString);
			        if (newValue != null)
			            sourceValue.Attribute(XmlConstants.ValueAttr).SetValue(newValue);
			    }
			}

            //var targetDimsRetyped = _targetDimensions.Select(d => new Data.Dimension { DimensionId = d.DimensionId, Key = d.ExternalKey }).ToList();
            //var sourceDimsRetyped = _sourceDimensions.Select(s => new Data.Dimension { DimensionId = s.DimensionId, Key = s.ExternalKey }).ToList();

            var importEntity = XmlToImportEntity.BuildEntityFromXml(entityNode, assignmentObjectTypeId,
				_targetDimensions, _sourceDimensions, _sourceDefaultDimensionId, DefaultLanguage, keyNumber, keyGuid, keyString);

			return importEntity;
		}

        /// <summary>
        /// Try to map a link like "file:275" from the import to the target system
        /// Will return null if nothing appropriate found, so the caller can choose to not do anything
        /// </summary>
        /// <param name="sourceValueString"></param>
        /// <returns></returns>
	    private string GetMappedLink(string sourceValueString)
	    {
            // file
	        var fileRegex = new Regex("^File:(?<Id>[0-9]+)", RegexOptions.IgnoreCase);
	        var a = fileRegex.Match(sourceValueString);

	        if (a.Success && a.Groups["Id"].Length > 0)
	        {
	            var originalId = int.Parse(a.Groups["Id"].Value);

	            if (_fileIdCorrectionList.ContainsKey(originalId))
	                return fileRegex.Replace(sourceValueString, "file:" + _fileIdCorrectionList[originalId]);
	        }

            // folder
	        var folderRegEx = new Regex("^folder:(?<Id>[0-9]+)", RegexOptions.IgnoreCase);
	        var f = folderRegEx.Match(sourceValueString);

	        if (f.Success && f.Groups["Id"].Length > 0)
	        {
	            var originalId = int.Parse(f.Groups["Id"].Value);

	            if (_folderIdCorrectionList.ContainsKey(originalId))
	                return folderRegEx.Replace(sourceValueString, "folder:" + _folderIdCorrectionList[originalId]);
	        }

	        return null;
	    }

	    #endregion
	}

    public class TemplateDefault
    {

        public string ItemType;
        public string ContentTypeStaticName;
        public int? DemoEntityId;

    }
}