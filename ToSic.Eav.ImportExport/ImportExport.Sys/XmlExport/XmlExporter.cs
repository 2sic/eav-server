using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Data.Ancestors.Sys;
using ToSic.Eav.Data.Entities.Sys.Lists;
using ToSic.Eav.Identity;
using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Sys;

namespace ToSic.Eav.ImportExport.Internal;
// this has a minimal risk of being different!
// should all get it from cache only!

[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class XmlExporter(XmlSerializer xmlSerializer, IAppsCatalog appsCatalog, string logPrefix, object[] connect)
    : ServiceBase(logPrefix + "XmlExp", connect: [..connect, appsCatalog, xmlSerializer])
{

    #region simple properties
    protected readonly List<int> ReferencedFileIds = [];
    protected readonly List<int> ReferencedFolderIds = [];
    public List<TenantFileItem> ReferencedFiles = [];
    private bool _isAppExport;

    public string[] ContentTypeNamesOrIds;
    public string[] EntityIDs;
    public List<Message> Messages = [];

    public IAppReader AppReader { get; private set; }
    public int ZoneId { get; private set; }

    private string _appStaticName = "";
    private AppExportSpecs _specs;
    private List<string> _foldersForEntitiesThatAreNotDeleted = [];
    #endregion

    #region Constructor & DI

    public XmlSerializer Serializer { get; } = xmlSerializer;
    protected readonly IAppsCatalog AppsCatalog = appsCatalog;

    protected void Constructor(AppExportSpecs specs, IAppReader appReader, string appStaticName, bool appExport, string[] typeNamesOrIds, string[] entityIds, string defaultCultureCode)
    {
        _specs = specs;
        ZoneId = specs.ZoneId;
        Log.A("start XML exporter using app-package");
        AppReader = appReader;
        Serializer.Init(
            AppsCatalog.Zone(specs.ZoneId).LanguagesActive
                .ToDictionary(
                    l => l.EnvironmentKey.ToLowerInvariant(),
                    l => l.DimensionId
                ),
            AppReader);

        _appStaticName = appStaticName;
        _isAppExport = appExport;
        ContentTypeNamesOrIds = typeNamesOrIds;
        EntityIDs = entityIds;

        // this must happen very early, to ensure that the file-lists etc. are correct for exporting when used externally
        InitExportXDocument(defaultCultureCode, EavSystemInfo.VersionString);
    }

    public abstract XmlExporter Init(AppExportSpecs specs, IAppReader appRuntime, bool appExport, string[] attrSetIds, string[] entityIds);

    ///// <summary>
    ///// Not that the overload of this must take care of creating the EavAppContext and calling the Constructor
    ///// </summary>
    ///// <returns></returns>
    //public virtual XmlExporter Init(AppExportSpecs specs, IAppReader appRuntime, bool appExport, string[] attrSetIds, string[] entityIds)
    //{
    //    ContextResolver.SetApp(new AppIdentity(specs.ZoneId, specs.AppId));
    //    var ctxOfApp = ContextResolver.AppRequired();
    //    PostContextInit(ctxOfApp);
    //    Constructor(specs, appRuntime, ctxOfApp.AppReader.Specs.NameId, appExport, attrSetIds, entityIds, ctxOfApp.Site.DefaultCultureCode);

    //    // this must happen very early, to ensure that the file-lists etc. are correct for exporting when used externally
    //    //InitExportXDocument(ctxOfApp.Site.DefaultCultureCode, EavSystemInfo.VersionString);

    //    return this;
    //}

    ///// <summary>
    ///// Post context init the caller must be able to init Adam, which is not part of this project, so we're handling it as a callback
    ///// </summary>
    ///// <param name="appContext"></param>
    //protected abstract void PostContextInit(IContextOfApp appContext);

    private void EnsureThisIsInitialized()
    {
        if (Serializer == null || string.IsNullOrEmpty(_appStaticName))
            throw new("Xml Exporter is not initialized - this is required before trying to export");
    }

    #endregion

    #region Export



    /// <summary>
    /// Exports given Content-Types, Entities and Templates to an XML and returns the XML as string.
    /// </summary>
    /// <returns></returns>
    public string GenerateNiceXml()
    {
        EnsureThisIsInitialized();

        var doc = ExportXDocument;

        // Will be used to show an export protocol in future
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

        var dimensions = AppsCatalog.Zone(ZoneId).LanguagesActive;

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

        var contentTypes = new XElement(XmlConstants.AttributeSets);

        // Go through each ContentTypeId
        foreach (var contentTypeId in ContentTypeNamesOrIds)
        {
            var set = int.TryParse(contentTypeId, out var id)
                ? AppReader.GetContentType(id)
                : AppReader.GetContentType(contentTypeId);  // in case it's the name, not the number

            // skip system/code-types
            if (set.HasPresetAncestor()) continue;

            var attributes = new XElement(XmlConstants.Attributes);

            // Add all Attributes to Content-Type/AttributeSet including meta information
            var appMetadata = AppReader.Metadata;
            foreach (var a in set.Attributes.OrderBy(a => a.SortOrder))
            {
                var xmlAttribute = new XElement(XmlConstants.Attribute,
                    new XAttribute(XmlConstants.Static, a.Name),
                    new XAttribute(XmlConstants.Type, a.Type.ToString()),
                    new XAttribute(XmlConstants.IsTitle, a.IsTitle),
                    // Add Attribute MetaData
                    appMetadata.GetMetadata(TargetTypes.Attribute, a.AttributeId)
                        .Select(c => GetEntityXElement(c.EntityId, c.Type.NameId))
                );

                // #SharedFieldDefinition
                if (a.Guid.HasValue) xmlAttribute.Add(new XAttribute(XmlConstants.Guid, a.Guid));
                if (a.SysSettings != null) xmlAttribute.Add(new XAttribute(XmlConstants.SysSettings, JsonSerializer.Serialize(a.SysSettings, Log)));

                attributes.Add(xmlAttribute);
            }

            // Add AttributeSet / Content Type
            var contentType = new XElement(XmlConstants.AttributeSet,
                new XAttribute(XmlConstants.Static, set.NameId),
                new XAttribute(XmlConstants.Name, set.Name),
                new XAttribute(XmlConstants.Scope, set.Scope),
                new XAttribute(XmlConstants.AlwaysShareConfig, set.AlwaysShareConfiguration),
                attributes);

            // Add Ghost-Info if content type inherits from another content type
            if (set.HasAncestor())
            {
                var parentStaticName = set.NameId;
                contentType.Add(new XAttribute(XmlConstants.AttributeSetParentDef, parentStaticName));
            }

            contentTypes.Add(contentType);
        }

        #endregion

        #region Entities

        var entities = new XElement(XmlConstants.Entities);

        // Go through each Entity
        foreach (var entityId in EntityIDs)
        {
            var id = int.Parse(entityId);

            // Get the entity and ContentType from ContentContext add Add it to ContentItems
            var entity = AppReader.List.FindRepoId(id);
            entities.Add(GetEntityXElement(entity.EntityId, entity.Type.NameId));
        }

        #endregion

        // init files (add to queue)
        AddFilesToExportQueue();

        GetFoldersForEntitiesThatAreNotDeleted(entities);

        // Create root node "SexyContent" and add ContentTypes, ContentItems and Templates
        doc.Add(new XElement(XmlConstants.RootNode,
            new XAttribute(XmlConstants.FileVersion, Settings.FileVersion),
            new XAttribute(XmlConstants.MinEnvVersion, Settings.MinimumRequiredDnnVersion),
            new XAttribute(XmlConstants.MinModVersion, moduleVersion),
            new XAttribute(XmlConstants.ExportDate, DateTime.Now),
            header,
            contentTypes,
            entities,
            GetFilesXElements(),
            GetFoldersXElements()));
    }

    /// <summary>
    /// Prepare list of possible discriminators for folder names in adam/app for assets of entities that are not deleted, to be exported with entity.
    /// Folders in adam/app that are not in this list are likely related to deleted entity, and probably can be skipped from export
    /// to keep size of export zips as small as possible and to not export unnecessary files.
    /// </summary>
    /// <param name="entities"></param>
    /// <remarks>
    /// GetFilesXElements and GetFoldersXElements depends on it.
    /// </remarks>
    private void GetFoldersForEntitiesThatAreNotDeleted(XElement entities)
    {
        // when export includes deleted assets, there is no need to check for deleted entities
        // so skip preparation of list of possible discriminators for folder names in adam/app
        if (_specs.AssetAdamDeleted)
        {
            _foldersForEntitiesThatAreNotDeleted = [];
            return;
        }

        // prepare list of compressed EntityGuids for Entities (that are not deleted)
        // to be used for validation of folders in adam/app for assets of entities that are not deleted
        var entityGuids = entities.Elements(XmlConstants.Entity).Select(e => (e.Attribute("EntityGUID"))?.Value).ToList();
        foreach (var entityGuid in entityGuids)
            if (Guid.TryParse(entityGuid, out var guid)) 
                _foldersForEntitiesThatAreNotDeleted.Add(guid.GuidCompress());
    }

    private XElement GetParentAppXElement()
    {
        if (!_isAppExport || _appStaticName == XmlConstants.AppContentGuid || !AppReader.HasCustomParentApp())
            return null;

        var parentAppState = AppReader.GetParentCache();
        return new(XmlConstants.ParentApp,
            new XAttribute(XmlConstants.Guid, parentAppState.NameId),
            new XAttribute(XmlConstants.AppId, parentAppState.AppId)
        );
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
            throw new("failed on entity id '" + entityId + "' of set-type '" + contentTypeName + "'", ex);
        }

        foreach (var value in entityXElement.Elements(XmlConstants.ValueNode))
        {
            var valueString = value.Attribute(XmlConstants.ValueAttr)?.Value;
            var valueType = value.Attribute(XmlConstants.EntityTypeAttribute)?.Value;
            var valueKey = value.Attribute(XmlConstants.KeyAttr)?.Value;

            if (string.IsNullOrEmpty(valueString)) continue;

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

    private XElement GetFilesXElements() => new(XmlConstants.PortalFiles /*"PortalFiles" */,
        ReferencedFileIds.Distinct().Select(GetFileXElement)
    );


    private XElement GetFoldersXElements()
    {
        return new(XmlConstants.FolderGroup,
            ReferencedFolderIds.Distinct().Select(GetFolderXElement)
        );
    }

    protected abstract TenantFileItem ResolveFile(int fileId);

    private XElement GetFileXElement(int fileId)
    {
        var file = ResolveFile(fileId);

        if (IsDeletedOrInvalid(file?.RelativePath)) return null;

        ReferencedFiles.Add(file);

        return new(XmlConstants.FileNode,
            new XAttribute(XmlConstants.FileIdAttr, file.Id),
            new XAttribute(XmlConstants.FolderNodePath, file.RelativePath)
        );
    }

    protected abstract string ResolveFolderId(int folderId);

    private XElement GetFolderXElement(int folderId)
    {
        var path = ResolveFolderId(folderId);

        if (IsDeletedOrInvalid(path)) return null;

        return new(XmlConstants.Folder,
            new XAttribute(XmlConstants.FolderNodeId, folderId),
            new XAttribute(XmlConstants.FolderNodePath, path)
        );
    }

    /// <summary>
    /// Checks if the asset path is invalid or belongs to deleted entity
    /// </summary>
    /// <param name="relativePath">The relative path of the asset.</param>
    /// <returns>True if the asset path is invalid or belongs to deleted entity, otherwise false.</returns>
    private bool IsDeletedOrInvalid(string relativePath)
    {
        // invalid, can't export assets without path
        if (relativePath == null) return true;

        // skip check for deleted assets and just leave it for export
        if (_specs.AssetAdamDeleted) return false;

        // if not in "adam" folder, we can't know is it deleted or not, so leave it for export
        // this is expected for site assets
        if (!relativePath.StartsWith("adam")) return false;

        // break adam path to parts,
        // this is necessary to check if it's in discriminator list of possible folders for entities that are not deleted
        var pathParts = relativePath.ForwardSlash().Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        // always export parent folders for entity assets (like adam/app/)
        if (pathParts.Length < 3) return false;

        // check if folder is in list of possible entities that are not deleted
        return _foldersForEntitiesThatAreNotDeleted.All(f => f != pathParts[2]);
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