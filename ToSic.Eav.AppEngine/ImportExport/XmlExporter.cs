using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Environment;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Repository.Efc.Parts;
using ExportImportMessage = ToSic.Eav.Persistence.Logging.ExportImportMessage;

namespace ToSic.Eav.Apps.ImportExport
{

    public abstract class XmlExporter
    {
        #region simple properties
        internal DbDataController EavAppContext;
        protected readonly List<int> ReferencedFileIds = new List<int>();
        protected readonly List<int> ReferencedFolderIds = new List<int>();
        public List<TennantFileItem> ReferencedFiles = new List<TennantFileItem>();
        private bool _isAppExport;


        public string[] AttributeSetIDs;
        public string[] EntityIDs;
        public List<ExportImportMessage> Messages = new List<ExportImportMessage>();


        #endregion

        #region Constructor stuff

        ///// <summary>
        ///// Initiate the exporter
        ///// </summary>
        ///// <param name="zoneId"></param>
        ///// <param name="appId"></param>
        //protected XmlExporter(int zoneId, int appId)
        //{
        //     EavAppContext = DbDataController.Instance(zoneId, appId); 
           
        //}


        private string _appStaticName = "";
        protected void Constructor(int zoneId, int appId, string appStaticName, bool appExport, string[] attrSetIds, string[] entityIds)
        {
            EavAppContext = DbDataController.Instance(zoneId, appId); 
            _appStaticName = appStaticName;
            _isAppExport = appExport;
            AttributeSetIDs = attrSetIds;
            EntityIDs = entityIds;
        }

        /// <summary>
        /// Not that the overload of this must take care of creating the EavAppContext and calling the Constructor
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        /// <param name="appExport"></param>
        /// <param name="attrSetIds"></param>
        /// <param name="entityIds"></param>
        /// <returns></returns>
        public abstract XmlExporter Init(int zoneId, int appId, bool appExport, string[] attrSetIds, string[] entityIds);

        private void EnsureThisIsInitialized()
        {
            if(EavAppContext == null || string.IsNullOrEmpty(_appStaticName))
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
            var doc = _exportDocument = new XmlBuilder().BuildDocument(); // before 2017-06-07 2dm: new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), null);

            #region Header

            var dimensions = EavAppContext.Dimensions.GetLanguages();//.GetDimensionChildren(Constants.CultureSystemKey);
            var header = new XElement(XmlConstants.Header,
                _isAppExport && _appStaticName != XmlConstants.AppContentGuid ? new XElement(XmlConstants.App,
                        new XAttribute(XmlConstants.Guid, _appStaticName)
                        )
                    : null,
                new XElement(XmlConstants.Language, new XAttribute(XmlConstants.LangDefault, defaultLanguage)),
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
            foreach (var attributeSetId in AttributeSetIDs)
            {
                var id = int.Parse(attributeSetId);
                var set = EavAppContext.AttribSet.GetDbAttribSet(id);
                var attributes = new XElement(XmlConstants.Attributes);

                // Add all Attributes to AttributeSet including meta informations
                foreach (var x in EavAppContext.AttributesDefinition.GetAttributesInSet(id))
                {
                    var attribute = new XElement(XmlConstants.Attribute,
                        new XAttribute(XmlConstants.Static, x.Attribute.StaticName),
                        new XAttribute(XmlConstants.Type, x.Attribute.Type),
                        new XAttribute(XmlConstants.IsTitle, x.IsTitle),
                        // Add Attribute MetaData
                        from c in
                            EavAppContext.Entities.GetAssignedEntities(Constants.MetadataForField, x.AttributeId).ToList()
                        select GetEntityXElement(c)
                        );

                    attributes.Add(attribute);
                }

                // Add AttributeSet / Content Type
                var attributeSet = new XElement(XmlConstants.AttributeSet,
                    new XAttribute(XmlConstants.Static, set.StaticName),
                    new XAttribute(XmlConstants.Name, set.Name),
                    new XAttribute(XmlConstants.Description, set.Description),
                    new XAttribute(XmlConstants.Scope, set.Scope),
                    new XAttribute(XmlConstants.AlwaysShareConfig, set.AlwaysShareConfiguration),
                    attributes);

                // Add Ghost-Info if content type inherits from another content type
                if (set.UsesConfigurationOfAttributeSet.HasValue)
                {
                    var parentAttributeSet =
                        EavAppContext.SqlDb.ToSicEavAttributeSets.First(
                            a =>
                                a.AttributeSetId == set.UsesConfigurationOfAttributeSet.Value &&
                                a.ChangeLogDeleted == null);
                    attributeSet.Add(new XAttribute(XmlConstants.AttributeSetParentDef, parentAttributeSet.StaticName));
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
                var entity = EavAppContext.Entities.GetDbEntity(id);
                entities.Add(GetEntityXElement(entity));
            }

            #endregion

            // init ADAM files (add to queue)
            // AddAdamFilesToExportQueue();
            AddFilesToExportQueue();

            // Create root node "SexyContent" and add ContentTypes, ContentItems and Templates
            doc.Add(new XElement(XmlConstants.RootNode,
                new XAttribute(XmlConstants.FileVersion, Settings.FileVersion),
                new XAttribute(XmlConstants.MinEnvVersion, Settings.MinimumRequiredVersion),
                new XAttribute(XmlConstants.MinModVersion, moduleVersion),
                new XAttribute(XmlConstants.ExportDate, DateTime.Now),
                header,
                attributeSets,
                entities,
                GetFilesXElements(),
                GetFoldersXElements()));
        }

        public abstract void AddFilesToExportQueue();

        /// <summary>
        /// Returns an Entity XElement
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private XElement GetEntityXElement(ToSicEavEntities e)
        {
            //Note that this often throws errors in a dev environment, where the data may be mangled manually in the DB
            XElement entityXElement;
            try
            {
                entityXElement = new DbXmlBuilder(EavAppContext).XmlEntity(e.EntityId);
            }
            catch (Exception ex)
            {
                throw new Exception("failed on entity id '" + e.EntityId + "' of set-type '" + e.AttributeSetId + "'", ex);
            }

            foreach (var value in entityXElement.Elements(XmlConstants.ValueNode))
            {
                var valueString = value.Attribute(XmlConstants.ValueAttr).Value;
                var valueType = value.Attribute(XmlConstants.EntityTypeAttribute).Value;
                var valueKey = value.Attribute(XmlConstants.KeyAttr).Value;

                // Special cases for Template ContentTypes
                if (e.AttributeSet.StaticName == XmlConstants.CtTemplate && !string.IsNullOrEmpty(valueString))
                {
                    switch (valueKey)
                    {
                        case XmlConstants.TemplateContentTypeId:
                            var attributeSet = EavAppContext.AttribSet.GetDbAttribSets().FirstOrDefault(a => a.AttributeSetId == int.Parse(valueString));
                            value.Attribute(XmlConstants.ValueAttr).SetValue(attributeSet != null ? attributeSet.StaticName : string.Empty);
                            break;
                        case XmlConstants.TemplateDemoItemId:
                            var entityId = int.Parse(valueString);
                            var demoEntity = EavAppContext.SqlDb.ToSicEavEntities.FirstOrDefault(en => en.EntityId == entityId);
                            value.Attribute(XmlConstants.ValueAttr).SetValue(demoEntity?.EntityGuid.ToString() ?? string.Empty);
                            break;
                    }
                }

                // Collect all referenced files for adding a file list to the xml later
                if (valueType == XmlConstants.ValueTypeLink)
                {
                    var fileRegex = new Regex(XmlConstants.FileRefRegex /*"^File:(?<FileId>[0-9]+)"*/, RegexOptions.IgnoreCase);
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
            return  new XElement(XmlConstants.FolderGroup,
                    ReferencedFolderIds.Distinct().Select(GetFolderXElement)
                );
        }

        protected abstract TennantFileItem ResolveFile(int fileId);

        private XElement GetFileXElement(int fileId)
        {
            var file = ResolveFile(fileId);

            if (file.RelativePath == null) return null;

            ReferencedFiles.Add(file);

            return new XElement(XmlConstants.FileNode,
                new XAttribute(XmlConstants.FileIdAttr, file.Id),
                new XAttribute(XmlConstants.FolderNodePath/*"RelativePath"*/, file.RelativePath)
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