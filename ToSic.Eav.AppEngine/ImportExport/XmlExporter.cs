using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using ToSic.Eav.BLL;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Environment;

namespace ToSic.Eav.Apps.ImportExport
{

    // todo: move all strings to XmlConstants



    public abstract class XmlExporter
    {
        #region simple properties
        internal EavDataController EavAppContext;
        protected readonly List<int> ReferencedFileIds = new List<int>();
        protected readonly List<int> ReferencedFolderIds = new List<int>();
        public List<TennantFileItem> ReferencedFiles = new List<TennantFileItem>();
        private bool _isAppExport;


        public string[] AttributeSetIDs;
        public string[] EntityIDs;
        public List<ExportImportMessage> Messages = new List<ExportImportMessage>();


        #endregion

        #region Constructor stuff
        /// <summary>
        /// Initiate the exporter
        /// </summary>
        /// <param name="zoneId"></param>
        /// <param name="appId"></param>
        protected XmlExporter(int zoneId, int appId)
        {
            EavAppContext = EavDataController.Instance(zoneId, appId); 
        }


        private string _appStaticName = "";
        protected void Constructor(string appStaticName, bool appExport, string[] attrSetIds, string[] entityIds)
        {
            _appStaticName = appStaticName;
            _isAppExport = appExport;
            AttributeSetIDs = attrSetIds;
            EntityIDs = entityIds;
        }

        #endregion

        #region Export



        /// <summary>
        /// Exports given AttributeSets, Entities and Templates to an XML and returns the XML as string.
        /// </summary>
        /// <returns></returns>
        public string GenerateNiceXml()
        {
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
            // Create XML document and declaration
            var doc = _exportDocument = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), null);

            #region Header

            var dimensions = EavAppContext.Dimensions.GetDimensionChildren("Culture");
            var header = new XElement(XmlConstants.Header,
                _isAppExport && _appStaticName != "Default"
                    ? new XElement(XmlConstants.App,
                        new XAttribute(XmlConstants.Guid, _appStaticName)
                        )
                    : null,
                new XElement("Language", new XAttribute("Default", defaultLanguage)),
                new XElement("Dimensions", dimensions.Select(d => new XElement("Dimension",
                    new XAttribute("DimensionID", d.DimensionID),
                    new XAttribute("Name", d.Name),
                    new XAttribute("SystemKey", d.SystemKey ?? String.Empty),
                    new XAttribute("ExternalKey", d.ExternalKey ?? String.Empty),
                    new XAttribute("Active", d.Active)
                    )))
                );

            #endregion

            #region Attribute Sets

            var attributeSets = new XElement("AttributeSets");

            // Go through each AttributeSetID
            foreach (var attributeSetId in AttributeSetIDs)
            {
                var id = int.Parse(attributeSetId);
                var set = EavAppContext.AttribSet.GetAttributeSet(id);
                var attributes = new XElement("Attributes");

                // Add all Attributes to AttributeSet including meta informations
                foreach (var x in EavAppContext.Attributes.GetAttributesInSet(id))
                {
                    var attribute = new XElement("Attribute",
                        new XAttribute(Const2.Static, x.Attribute.StaticName),
                        new XAttribute(Const2.Type, x.Attribute.Type),
                        new XAttribute(Const2.IsTitle, x.IsTitle),
                        // Add Attribute MetaData
                        from c in
                            EavAppContext.Entities.GetEntities(ToSic.Eav.Constants.AssignmentObjectTypeIdFieldProperties,
                                x.AttributeID).ToList()
                        select GetEntityXElement(c)
                        );

                    attributes.Add(attribute);
                }

                // Add AttributeSet / Content Type
                var attributeSet = new XElement("AttributeSet",
                    new XAttribute(Const2.Static, set.StaticName),
                    new XAttribute(Const2.Name, set.Name),
                    new XAttribute(Const2.Description, set.Description),
                    new XAttribute(Const2.Scope, set.Scope),
                    new XAttribute(Const2.AlwaysShareConfig, set.AlwaysShareConfiguration),
                    attributes);

                // Add Ghost-Info if content type inherits from another content type
                if (set.UsesConfigurationOfAttributeSet.HasValue)
                {
                    var parentAttributeSet =
                        EavAppContext.SqlDb.AttributeSets.First(
                            a =>
                                a.AttributeSetID == set.UsesConfigurationOfAttributeSet.Value &&
                                a.ChangeLogDeleted == null);
                    attributeSet.Add(new XAttribute("UsesConfigurationOfAttributeSet", parentAttributeSet.StaticName));
                }

                attributeSets.Add(attributeSet);
            }

            #endregion

            #region Entities

            var entities = new XElement("Entities");

            // Go through each Entity
            foreach (var entityId in EntityIDs)
            {
                var id = int.Parse(entityId);

                // Get the entity and ContentType from ContentContext add Add it to ContentItems
                var entity = EavAppContext.Entities.GetEntity(id);
                entities.Add(GetEntityXElement(entity));
            }

            #endregion

            // init ADAM files (add to queue)
            // AddAdamFilesToExportQueue();
            AddFilesToExportQueue();

            // Create root node "SexyContent" and add ContentTypes, ContentItems and Templates
            doc.Add(new XElement(XmlConstants.RootNode,
                new XAttribute("FileVersion", Settings.FileVersion),
                new XAttribute("MinimumRequiredVersion", Settings.MinimumRequiredVersion),
                new XAttribute("ModuleVersion", moduleVersion),
                new XAttribute("ExportDate", DateTime.Now),
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
        private XElement GetEntityXElement(Entity e)
        {
            //Note that this often throws errors in a dev environment, where the data may be mangled manually in the DB
            XElement entityXElement;
            try
            {
                entityXElement = new XmlNodeBuilder(EavAppContext).GetEntityXElement(e.EntityID);
            }
            catch (Exception ex)
            {
                throw new Exception("failed on entity id '" + e.EntityID + "' of set-type '" + e.AttributeSetID + "'", ex);
            }

            foreach (var value in entityXElement.Elements("Value"))
            {
                var valueString = value.Attribute("Value").Value;
                var valueType = value.Attribute("Type").Value;
                var valueKey = value.Attribute("Key").Value;

                // Special cases for Template ContentTypes
                if (e.Set.StaticName == "2SexyContent-Template-ContentTypes" && !string.IsNullOrEmpty(valueString))
                {
                    switch (valueKey)
                    {
                        case "ContentTypeID":
                            var attributeSet = EavAppContext.AttribSet.GetAllAttributeSets().FirstOrDefault(a => a.AttributeSetID == int.Parse(valueString));
                            value.Attribute("Value").SetValue(attributeSet != null ? attributeSet.StaticName : string.Empty);
                            break;
                        case "DemoEntityID":
                            var entityId = int.Parse(valueString);
                            var demoEntity = EavAppContext.SqlDb.Entities.FirstOrDefault(en => en.EntityID == entityId);
                            value.Attribute("Value").SetValue(demoEntity?.EntityGUID.ToString() ?? string.Empty);
                            break;
                    }
                }

                // Collect all referenced files for adding a file list to the xml later
                if (valueType == "Hyperlink")
                {
                    var fileRegex = new Regex("^File:(?<FileId>[0-9]+)", RegexOptions.IgnoreCase);
                    var a = fileRegex.Match(valueString);
                    // try remember the file
                    if (a.Success && a.Groups["FileId"].Length > 0)
                        AddFileAndFolderToQueue(int.Parse(a.Groups["FileId"].Value));
                }
            }

	        if (e.KeyGuid.HasValue)
		        entityXElement.Add(new XAttribute("KeyGuid", e.KeyGuid));

            if (e.KeyNumber.HasValue)
                entityXElement.Add(new XAttribute("KeyNumber", e.KeyNumber));
            if (!string.IsNullOrEmpty(e.KeyString))
                entityXElement.Add(new XAttribute("KeyString", e.KeyString));

            //return new XElement("Entity",
            //    new XAttribute("AssignmentObjectType", e.AssignmentObjectType.Name),
            //    new XAttribute("AttributeSetStaticName", attributeSet.StaticName),
            //    new XAttribute("AttributeSetName", attributeSet.Name),
            //    new XAttribute("EntityGUID", e.EntityGUID),
            //    from c in Sexy.ContentContext.GetValues(e.EntityID)
            //    where c.ChangeLogDeleted == null
            //    select GetAttributeValueXElement(c.Attribute.StaticName, c, c.Attribute.Type, attributeSet));

            return entityXElement;
        }

        protected abstract void AddFileAndFolderToQueue(int fileNum);


        #endregion

        #region Files & Pages

        private XElement GetFilesXElements() => new XElement("PortalFiles",
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

            return new XElement("File",
                new XAttribute("Id", file.Id),
                new XAttribute("RelativePath", file.RelativePath)
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
        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }
    }
}