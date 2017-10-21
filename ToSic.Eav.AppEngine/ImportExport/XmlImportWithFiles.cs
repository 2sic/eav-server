using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Repository.Efc;

// 2dm: must disable NullRef warnings, because there a lot of warnings when processing XML, 
// ...and these are real errors which should blow
// ReSharper disable PossibleNullReferenceException

namespace ToSic.Eav.Apps.ImportExport
{
    public class XmlImportWithFiles: HasLog
	{
		public List<Message> Messages;

        private List<DimensionDefinition> _targetDimensions;
        private DbDataController _eavContext;
		public int AppId { get; private set; }
		public int ZoneId { get; private set; }
		private readonly Dictionary<int, int> _fileIdCorrectionList = new Dictionary<int, int>();
	    private readonly Dictionary<int, int> _folderIdCorrectionList = new Dictionary<int, int>();

        private readonly IImportExportEnvironment _environment;

	    private XmlToEntity _xmlBuilder;

        /// <summary>
        /// The default language / culture - example: de-DE
        /// </summary>
        private string DefaultLanguage { get; }

        private bool AllowSystemChanges { get; }

		#region Prerequisites

	    /// <summary>
	    /// Create a new xmlImport instance
	    /// </summary>
	    /// <param name="parentLog"></param>
	    /// <param name="defaultLanguage">The portals default language / culture - example: de-DE</param>
	    /// <param name="allowSystemChanges">Specify if the import should be able to change system-wide things like shared attributesets</param>
	    public XmlImportWithFiles(Log parentLog, string defaultLanguage = null, bool allowSystemChanges = false): base("Xml.ImpFil", parentLog)
		{
		    _environment = Factory.Resolve<IImportExportEnvironment>();
            _environment.LinkLog(Log);
			// Prepare
			Messages = new List<Message>();
		    DefaultLanguage = (defaultLanguage ?? _environment.DefaultLanguage).ToLowerInvariant();
			AllowSystemChanges = allowSystemChanges;
        }

		public bool IsCompatible(XDocument doc)
		{
		    Log.Add("is compatible check");
		    var rns = doc.Elements(XmlConstants.RootNode);
		    var rn = doc.Element(XmlConstants.RootNode);
			// Return if no Root Node "SexyContent"
			if (!rns.Any() || rn == null)
			{
				Messages.Add(new Message("The XML file you specified does not seem to be a 2sxc Export.", Message.MessageTypes.Error));
				return false;
			}
			// Return if Version does not match
			if (rn.Attributes().All(a => a.Name != XmlConstants.MinEnvVersion) || new Version(rn.Attribute(XmlConstants.MinEnvVersion).Value) > new Version(_environment.ModuleVersion))
			{
				Messages.Add(new Message("This template or app requires version " + rn.Attribute(XmlConstants.MinEnvVersion).Value + " in order to work, you have version " + _environment.ModuleVersion + " installed.", Message.MessageTypes.Error));
				return false;
			}

		    Log.Add("is compatible completed");
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
            _environment.CreateFoldersAndMapToImportIds(foldersAndPath, _folderIdCorrectionList, Messages);
        }
	    #endregion

        /// <summary>
        /// Creates an app and then imports the xml
        /// </summary>
        /// <returns>AppId of the new imported app</returns>
        public bool ImportApp(int zoneId, XDocument doc, out int/*?*/ appId)
        {
            Log.Add($"import app z#{zoneId}");
			// Increase script timeout to prevent timeouts
			//HttpContext.Current.Server.ScriptTimeout = 300;

			// appId = new int?();
            appId = 0;

			if (!IsCompatible(doc))
			{
				Messages.Add(new Message("The import file is not compatible with the installed version of 2sxc.", Message.MessageTypes.Error));
				return false;
			}

			// Get root node "SexyContent"
			var xmlSource = doc.Element(XmlConstants.RootNode);
			var xApp = xmlSource?.Element(XmlConstants.Header)?.Element(XmlConstants.App);

			var appGuid = xApp?.Attribute(XmlConstants.Guid)?.Value;

            if (appGuid == null)
            {
                Messages.Add(new Message("Something is wrong in the xml structure, can't get an app-guid", Message.MessageTypes.Error));
                return false;
            }

            if (appGuid != XmlConstants.AppContentGuid)
            {
                // Build Guid (take existing, or create a new)
                if (String.IsNullOrEmpty(appGuid) || appGuid == new Guid().ToString())
                    appGuid = Guid.NewGuid().ToString();

                // Adding app to EAV
                var eavDc = DbDataController.Instance(zoneId, parentLog: Log);
                var app = eavDc.App.AddApp(null, appGuid);
                eavDc.SqlDb.SaveChanges();

                appId = app.AppId;
            }
            else
                appId = AppId;

            if (appId <= 0)
			{
				Messages.Add(new Message("App was not created. Please try again or make sure the package you are importing is correct.", Message.MessageTypes.Error));
				return false;
			}

            DataSource.GetCache(null).PurgeGlobalCache();   // must do this, to ensure that the app-id exists now 
            Log.Add("import app completed");
			return ImportXml(zoneId, appId/*.Value*/, doc);
		}

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

            _xmlBuilder = new XmlToEntity(AppId, sourceDimensions, sourceDefaultDimensionId, _targetDimensions, DefaultLanguage);
            #endregion

            var atsNodes = xmlSource.Element(XmlConstants.AttributeSets)?.Elements(XmlConstants.AttributeSet);
		    var entNodes = xmlSource.Elements(XmlConstants.Entities).Elements(XmlConstants.Entity);

            var importAttributeSets = GetImportAttributeSets(atsNodes);
		    var importEntities = GetImportEntities(entNodes, Constants.NotMetadata);


			var import = new Import(ZoneId, AppId, leaveExistingValuesUntouched);
			import.ImportIntoDb(importAttributeSets, importEntities.Cast<Entity>());
            SystemManager.Purge(ZoneId, AppId);

			Messages.AddRange(GetExportImportMessagesFromImportLog(import.Storage.ImportLogToBeRefactored));

			if (xmlSource.Elements(XmlConstants.Templates).Any())
				ImportXmlTemplates(xmlSource);

		    Log.Add("import xml completed");
			return true;
		}

	    private static List<DimensionDefinition> BuildSourceDimensionsList(XElement xmlSource)
	    {
	        var sDimensions =
	            xmlSource.Element(XmlConstants.Header)?
	                .Element(XmlConstants.DimensionDefinition)?
	                .Elements(XmlConstants.DimensionDefElement)
	                .Select(p => new DimensionDefinition()
	                {
	                    DimensionId = int.Parse(p.Attribute(XmlConstants.DimId).Value),
	                    Name = p.Attribute(XmlConstants.Name).Value,
	                    Key = p.Attribute(XmlConstants.CultureSysKey).Value,
	                    EnvironmentKey = p.Attribute(XmlConstants.CultureExtKey).Value,
	                    Active = Boolean.Parse(p.Attribute(XmlConstants.CultureIsActiveAttrib).Value)
	                }).ToList();
	        return sDimensions;
	    }

	    /// <summary>
        /// Maps EAV import messages to 2sxc import messages
        /// </summary>
        /// <param name="importLog"></param>
        /// <returns></returns>
        private IEnumerable<Message> GetExportImportMessagesFromImportLog(List<LogItem> importLog)
            => importLog.Select(l => new Message(l.Message,
                l.EntryType == EventLogEntryType.Error
                    ? Message.MessageTypes.Error
                    : l.EntryType == EventLogEntryType.Information
                        ? Message.MessageTypes.Information
                        : Message.MessageTypes.Warning
                ));
		

		#region AttributeSets

		private List<ContentType> GetImportAttributeSets(IEnumerable<XElement> xAttributeSets)
		{
		    Log.Add("get imp attrib sets");
            var importAttributeSets = new List<ContentType>();

			// Loop through AttributeSets
			foreach (var attributeSet in xAttributeSets)
			{
				var attributes = new List<IAttributeDefinition>();
			    var attsetElem = attributeSet.Element(XmlConstants.Attributes);
                if (attsetElem != null)
                    foreach (var xElementAttribute in attsetElem.Elements(XmlConstants.Attribute))
                    {
                        var attribute = new AttributeDefinition(AppId,
                            xElementAttribute.Attribute(XmlConstants.Static).Value,
                            null,
                            xElementAttribute.Attribute(XmlConstants.EntityTypeAttribute).Value,
                            null, null, null, null
                        );
                        attribute.AddItems(GetImportEntities(xElementAttribute.Elements(XmlConstants.Entity), Constants.MetadataForAttribute));
                        attributes.Add(attribute);

                        // Set Title Attribute
                        if (Boolean.Parse(xElementAttribute.Attribute(XmlConstants.IsTitle).Value))
                            attribute.IsTitle = true;
                    }
                // check if it's normal (not a ghost) but still missing a title
			    if(attributes.Any() && !attributes.Any(a => a.IsTitle)) 
			        (attributes.First() as AttributeDefinition).IsTitle = true;

			    // Add AttributeSet
                var ct = new ContentType(AppId, attributeSet.Attribute(XmlConstants.Name).Value)
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
            Log.Add("import xml templates");
            var templates = root.Element(XmlConstants.Templates);
            if (templates == null) return;

            var cache = DataSource.GetCache(ZoneId, AppId);

            foreach (var template in templates.Elements(XmlConstants.Template))
            {
                var name = "";
                try
                {
                    name = template.Attribute(XmlConstants.Name).Value;
                    var path = template.Attribute(AppConstants.TemplatePath).Value;

                    var contentTypeStaticName = template.Attribute(XmlConstants.AttSetStatic).Value;

                    Log.Add($"template:{name}, type:{contentTypeStaticName}, path:{path}");

                    if (!String.IsNullOrEmpty(contentTypeStaticName) && cache.GetContentType(contentTypeStaticName) == null)
                    {
                        Messages.Add(new Message($"Content Type for Template \'{name}\' could not be found. The template has not been imported.",
                                Message.MessageTypes.Warning));
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
                            Messages.Add(new Message($"Demo Entity for Template \'{name}\' could not be found. (Guid: {demoEntityGuid})", Message.MessageTypes.Information));

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
                            Messages.Add(new Message($"Pipeline Entity for Template \'{name}\' could not be found. (Guid: {pipelineEntityGuid.Value})", Message.MessageTypes.Information));
                    }

                    var useForList = false;
                    if (template.Attribute(AppConstants.TemplateUseList) != null)
                        useForList = Boolean.Parse(template.Attribute(AppConstants.TemplateUseList).Value);

                    var lstTemplateDefaults = template.Elements(XmlConstants.Entity).Select(e =>
                    {
                        var xmlItemType =
                            e.Elements(XmlConstants.ValueNode)
                                .FirstOrDefault(v =>
                                    v.Attribute(XmlConstants.KeyAttr).Value == XmlConstants.TemplateItemType)?
                                .Attribute(XmlConstants.ValueAttr)
                                .Value;
                        var xmlContentTypeStaticName =
                            e.Elements(XmlConstants.ValueNode)
                                .FirstOrDefault(v =>
                                    v.Attribute(XmlConstants.KeyAttr).Value == XmlConstants.TemplateContentTypeId)?
                                .Attribute(XmlConstants.ValueAttr)
                                .Value;
                        var xmlDemoEntityGuidString =
                            e.Elements(XmlConstants.ValueNode)
                                .FirstOrDefault(v =>
                                    v.Attribute(XmlConstants.KeyAttr).Value == XmlConstants.TemplateDemoItemId)?
                                .Attribute(XmlConstants.ValueAttr)
                                .Value;
                        if (xmlItemType == null || xmlContentTypeStaticName == null || xmlDemoEntityGuidString == null)
                        {
                            Messages.Add(new Message(
                                $"trouble with template '{name}' - either type, static or guid are null",
                                Message.MessageTypes.Error));
                            return null;
                        }
                        var xmlDemoEntityId = new int?();
                        if (xmlDemoEntityGuidString != "0" && xmlDemoEntityGuidString != "")
                        {
                            var xmlDemoEntityGuid = Guid.Parse(xmlDemoEntityGuidString);
                            if (_eavContext.Entities.EntityExists(xmlDemoEntityGuid))
                                xmlDemoEntityId = _eavContext.Entities.GetMostCurrentDbEntity(xmlDemoEntityGuid)
                                    .EntityId;
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
                    var presentationDefault = templateDefaults.FirstOrDefault(t => t.ItemType == AppConstants.Presentation);
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

                    new AppManager(_eavContext.ZoneId, _eavContext.AppId, Log).Templates.CreateOrUpdate(
                        null, name, path, contentTypeStaticName, demoEntityId, presentationTypeStaticName,
                        presentationDemoEntityId, listContentTypeStaticName, listContentDemoEntityId,
                        listPresentationTypeStaticName, listPresentationDemoEntityId, type, isHidden, location,
                        useForList, publishData, streamsToPublish, pipelineEntityId, viewNameInUrl);

                    Messages.Add(new Message($"Template \'{name}\' successfully imported.",
                        Message.MessageTypes.Information));
                }

                catch (Exception)
                {
                    Messages.Add(new Message($"Import for template \'{name}\' failed.",
                        Message.MessageTypes.Information));
                }

            }
            Log.Add("import xml templates - completed");
        }

        #endregion

		#region Entities

        /// <summary>
        /// Returns a collection of EAV import entities
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="assignmentObjectTypeId"></param>
        /// <returns></returns>
        private List<IEntity> GetImportEntities(IEnumerable<XElement> entities, int assignmentObjectTypeId)
            => entities.Select(e => GetImportEntity(e, assignmentObjectTypeId)).ToList();
		


        /// <summary>
        /// Returns an EAV import entity
        /// </summary>
        /// <param name="entityNode">The xml-Element of the entity to import</param>
        /// <param name="assignmentObjectTypeId">assignmentObjectTypeId</param>
        /// <returns></returns>
        private IEntity GetImportEntity(XElement entityNode, int assignmentObjectTypeId)
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
					keyNumber = AppId;
					assignmentObjectTypeId = SystemRuntime.MetadataType(Constants.AppAssignmentName);
					break;
                case XmlConstants.Entity:
                case "Data Pipeline": // 2dm: this was an old value, 2017-08-11 this was still used in the old Employees directory app v. 1.02
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

            var importEntity = _xmlBuilder.BuildEntityFromXml(entityNode, new Metadata
                {
                    TargetType = assignmentObjectTypeId,
                    KeyNumber = keyNumber,
                    KeyGuid = keyGuid,
                    KeyString = keyString
                });

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