using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Environment;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Persistence.Xml;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.ImportExport
{
    // this has a minimal risk of being different!
    // should all get it from cache only!

    public abstract class XmlExporter : ServiceBase
    {

        #region simple properties
        protected readonly List<int> ReferencedFileIds = new List<int>();
        protected readonly List<int> ReferencedFolderIds = new List<int>();
        public List<TenantFileItem> ReferencedFiles = new List<TenantFileItem>();
        private bool _isAppExport;

        public string[] AttributeSetNamesOrIds;
        public string[] EntityIDs;
        public List<Message> Messages = new List<Message>();

        public AppState AppState { get; private set; }

        public int ZoneId { get; private set; }

        private string _appStaticName = "";
        #endregion

        #region Constructor & DI

        protected XmlExporter(XmlSerializer xmlSerializer, IAppStates appStates, string logPrefix) : base(logPrefix + "XmlExp")
        {
            ConnectServices(
                AppStates = appStates,
                Serializer = xmlSerializer
            );
        }
        public XmlSerializer Serializer { get; }
        protected readonly IAppStates AppStates;

        protected void Constructor(int zoneId, AppRuntime app, string appStaticName, bool appExport, string[] typeNamesOrIds, string[] entityIds, ILog parentLog)
        {
            ZoneId = zoneId;
            this.Init(parentLog);
            Log.A("start XML exporter using app-package");
            AppState = app.AppState;
            Serializer.Init(Log).Init(AppStates.Languages(zoneId).ToDictionary(l => l.EnvironmentKey.ToLowerInvariant(), l => l.DimensionId),
                AppState);

            _appStaticName = appStaticName;
            _isAppExport = appExport;
            AttributeSetNamesOrIds = typeNamesOrIds;
            EntityIDs = entityIds;
        }


        /// <summary>
        /// Not that the overload of this must take care of creating the EavAppContext and calling the Constructor
        /// </summary>
        /// <returns></returns>
        public abstract XmlExporter Init(int zoneId, int appId, AppRuntime appRuntime, bool appExport, string[] attrSetIds, string[] entityIds, ILog parentLog);

        private void EnsureThisIsInitialized()
        {
            if (Serializer == null || string.IsNullOrEmpty(_appStaticName))
                throw new Exception("Xml Exporter is not initialized - this is required before trying to export");
        }

        #endregion

        #region Export



        /// <summary>
        /// Exports given AttributeSets, Entities and Templates to an XML and returns the XML as string.
        /// </summary>
        /// <returns></returns>
        public string GenerateNiceXml()
        {
            EnsureThisIsInitialized();

            var doc = ExportXDocument;

            // Will be used to show an export protocoll in future
            Messages = null;

            // Write XDocument to string and return it
            var xmlSettings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                ConformanceLevel = ConformanceLevel.Document,
                Indent = true
            };

            using (var stringWriter = new Utf8StringWriter())
            {
                using (var writer = XmlWriter.Create(stringWriter, xmlSettings))
                    doc.Save(writer);
                return stringWriter.ToString();
            }
        }

        private XDocument _exportDocument;

        public XDocument ExportXDocument => _exportDocument;

        protected void InitExportXDocument(string defaultLanguage, string moduleVersion)
        {
            EnsureThisIsInitialized();

            // Create XML document and declaration
            var doc = _exportDocument = new XmlBuilder().BuildDocument();

            #region Header

            var dimensions = AppStates.Languages(ZoneId); // new ZoneRuntime().Init(ZoneId, Log).Languages();

            var header = new XElement(XmlConstants.Header,
               _isAppExport && _appStaticName != XmlConstants.AppContentGuid
                   ? new XElement(XmlConstants.App, new XAttribute(XmlConstants.Guid, _appStaticName))
                   : null,
                GetParentAppXElement(),
               // Default Language of this site
               new XElement(XmlConstants.Language, new XAttribute(XmlConstants.LangDefault, defaultLanguage)),
               // All languages of this site/export
               new XElement(XmlConstants.DimensionDefinition, dimensions.Select(d => new XElement(XmlConstants.DimensionDefElement,
                   new XAttribute(XmlConstants.DimId, d.DimensionId),
                   new XAttribute(XmlConstants.Name, d.Name),
                   new XAttribute(XmlConstants.CultureSysKey, d.Key ?? string.Empty),
                   new XAttribute(XmlConstants.CultureExtKey, d.EnvironmentKey ?? string.Empty),
                   new XAttribute(XmlConstants.CultureIsActiveAttrib, d.Active)
                   )))
               );

            #endregion

            #region Attribute Sets

            var attributeSets = new XElement(XmlConstants.AttributeSets);

            // Go through each AttributeSetID
            foreach (var attributeSetId in AttributeSetNamesOrIds)
            {
                var set = int.TryParse(attributeSetId, out var id)
                    ? AppState.GetContentType(id)
                    : AppState.GetContentType(attributeSetId);  // in case it's the name, not the number

                // skip system/code-types
                if (set.HasPresetAncestor()) // ((set.ParentId ?? 0) == Constants.PresetContentTypeFakeParent)
                    continue;

                var attributes = new XElement(XmlConstants.Attributes);

                // Add all Attributes to AttributeSet including meta information
                foreach (var x in set.Attributes.OrderBy(a => a.SortOrder))
                {
                    var attribute = new XElement(XmlConstants.Attribute,
                        new XAttribute(XmlConstants.Static, x.Name),
                        new XAttribute(XmlConstants.Type, x.Type),
                        new XAttribute(XmlConstants.IsTitle, x.IsTitle),
                        // Add Attribute MetaData
                        from c in AppState.GetMetadata(TargetTypes.Attribute, x.AttributeId).ToList()
                        select GetEntityXElement(c.EntityId, c.Type.NameId)
                    );

                    attributes.Add(attribute);
                }

                // Add AttributeSet / Content Type
                var attributeSet = new XElement(XmlConstants.AttributeSet,
                    new XAttribute(XmlConstants.Static, set.NameId),
                    new XAttribute(XmlConstants.Name, set.Name),
                    // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
                    // new XAttribute(XmlConstants.Description, set.Description),
                    new XAttribute(XmlConstants.Scope, set.Scope),
                    new XAttribute(XmlConstants.AlwaysShareConfig, set.AlwaysShareConfiguration),
                    attributes);

                // Add Ghost-Info if content type inherits from another content type
                if (set.HasAncestor()) // .ParentId.HasValue)
                {
                    var parentStaticName = set.NameId;
                    attributeSet.Add(new XAttribute(XmlConstants.AttributeSetParentDef, parentStaticName));
                }

                attributeSets.Add(attributeSet);
            }

            #endregion

            #region Entities

            var entities = new XElement(XmlConstants.Entities);

            // Go through each Entity
            foreach (var entityId in EntityIDs)
            {
                var id = int.Parse(entityId);

                // Get the entity and ContentType from ContentContext add Add it to ContentItems
                var entity = AppState.List.FindRepoId(id);
                entities.Add(GetEntityXElement(entity.EntityId, entity.Type.NameId));
            }

            #endregion

            // init files (add to queue)
            AddFilesToExportQueue();

            // Create root node "SexyContent" and add ContentTypes, ContentItems and Templates
            doc.Add(new XElement(XmlConstants.RootNode,
                new XAttribute(XmlConstants.FileVersion, Settings.FileVersion),
                new XAttribute(XmlConstants.MinEnvVersion, Settings.MinimumRequiredDnnVersion),
                new XAttribute(XmlConstants.MinModVersion, moduleVersion),
                new XAttribute(XmlConstants.ExportDate, DateTime.Now),
                header,
                attributeSets,
                entities,
                GetFilesXElements(),
                GetFoldersXElements()));
        }

        private XElement GetParentAppXElement()
        {
            if (_isAppExport && _appStaticName != XmlConstants.AppContentGuid)
            {
                if (AppState.HasCustomParentApp())
                    return new XElement(XmlConstants.ParentApp,
                        new XAttribute(XmlConstants.Guid, AppState.ParentApp.AppState?.NameId),
                        new XAttribute(XmlConstants.AppId, AppState.ParentApp.AppState?.AppId)
                    );
            }
            return null;
        }

        public abstract void AddFilesToExportQueue();

        /// <summary>
        /// Returns an Entity XElement
        /// </summary>
        /// <returns></returns>
        private XElement GetEntityXElement(int entityId, string contentTypeName)
        {
            //Note that this often throws errors in a dev environment, where the data may be mangled manually in the DB
            XElement entityXElement;
            try
            {
                entityXElement = Serializer.ToXml(entityId);
            }
            catch (Exception ex)
            {
                throw new Exception("failed on entity id '" + entityId + "' of set-type '" + contentTypeName + "'", ex);
            }

            foreach (var value in entityXElement.Elements(XmlConstants.ValueNode))
            {
                var valueString = value.Attribute(XmlConstants.ValueAttr)?.Value;
                var valueType = value.Attribute(XmlConstants.EntityTypeAttribute)?.Value;
                var valueKey = value.Attribute(XmlConstants.KeyAttr)?.Value;

                if (string.IsNullOrEmpty(valueString)) continue;

                // 2022-01-04 2dm Disabled this, as it's for super-old way of storing template information
                // I'm pretty sure nothing in use has this in a way that would ever need exporting again, 
                // And nothing in use could handle an upgrade to the current version
                // Keep this commented till End of June 2022 #cleanUp #oldTemplates #2631
                //// Special cases for Template ContentTypes
                //if (contentTypeName == XmlConstants.CtTemplate)
                //{
                //    switch (valueKey)
                //    {
                //        case XmlConstants.TemplateContentTypeId:
                //            var eid = int.Parse(valueString);
                //            var attributeSet = AppState.GetContentType(eid);
                //            value.Attribute(XmlConstants.ValueAttr)?.SetValue(attributeSet != null ? attributeSet.StaticName : string.Empty);
                //            break;
                //        case XmlConstants.TemplateDemoItemId:
                //            eid = int.Parse(valueString);
                //            var demoEntity = AppState.List.FindRepoId(eid);
                //            value.Attribute(XmlConstants.ValueAttr)?.SetValue(demoEntity?.EntityGuid.ToString() ?? string.Empty);
                //            break;
                //    }
                //}

                // Collect all referenced files for adding a file list to the xml later
                if (valueType == XmlConstants.ValueTypeLink)
                {
                    var fileRegex = new Regex(XmlConstants.FileRefRegex, RegexOptions.IgnoreCase);
                    var a = fileRegex.Match(valueString);
                    // try remember the file
                    if (a.Success && a.Groups[XmlConstants.FileIdInRegEx].Length > 0)
                        AddFileAndFolderToQueue(int.Parse(a.Groups[XmlConstants.FileIdInRegEx].Value));
                }
            }

            return entityXElement;
        }

        protected abstract void AddFileAndFolderToQueue(int fileNum);


        #endregion

        #region Files & Pages

        private XElement GetFilesXElements() => new XElement(XmlConstants.PortalFiles /*"PortalFiles" */,
            ReferencedFileIds.Distinct().Select(GetFileXElement)
        );


        private XElement GetFoldersXElements()
        {
            return new XElement(XmlConstants.FolderGroup,
                    ReferencedFolderIds.Distinct().Select(GetFolderXElement)
                );
        }

        protected abstract TenantFileItem ResolveFile(int fileId);

        private XElement GetFileXElement(int fileId)
        {
            var file = ResolveFile(fileId);

            if (file.RelativePath == null) return null;

            ReferencedFiles.Add(file);

            return new XElement(XmlConstants.FileNode,
                new XAttribute(XmlConstants.FileIdAttr, file.Id),
                new XAttribute(XmlConstants.FolderNodePath, file.RelativePath)
            );
        }

        protected abstract string ResolveFolderId(int folderId);

        private XElement GetFolderXElement(int folderId)
        {
            var path = ResolveFolderId(folderId);

            if (path != null)
            {
                return new XElement(XmlConstants.Folder,
                        new XAttribute(XmlConstants.FolderNodeId, folderId),
                        new XAttribute(XmlConstants.FolderNodePath, path)
                    );
            }

            return null;
        }
        #endregion

        /// <summary>
        /// Creates a new StringWriter with UTF8 Encoding
        /// </summary>
        internal class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}