using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Context;
using ToSic.Eav.Context.Internal;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.ImportExport.Internal;
// this has a minimal risk of being different!
// should all get it from cache only!

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class XmlExporter : ServiceBase
{

    #region simple properties
    protected readonly List<int> ReferencedFileIds = [];
    protected readonly List<int> ReferencedFolderIds = [];
    public List<TenantFileItem> ReferencedFiles = [];
    private bool _isAppExport;

    public string[] AttributeSetNamesOrIds;
    public string[] EntityIDs;
    public List<Message> Messages = [];

    public IAppStateInternal AppState { get; private set; }

    public int ZoneId { get; private set; }

    private string _appStaticName = "";
    #endregion

    #region Constructor & DI

    protected XmlExporter(XmlSerializer xmlSerializer, IAppStates appStates, IContextResolver contextResolver, string logPrefix) : base(logPrefix + "XmlExp")
    {
        ConnectServices(
            AppStates = appStates,
            Serializer = xmlSerializer,
            ContextResolver = contextResolver
        );
    }
    protected IContextResolver ContextResolver { get; }
    public XmlSerializer Serializer { get; }
    protected readonly IAppStates AppStates;

    protected void Constructor(int zoneId, IAppStateInternal appState, string appStaticName, bool appExport, string[] typeNamesOrIds, string[] entityIds)
    {
        ZoneId = zoneId;
        Log.A("start XML exporter using app-package");
        AppState = appState;
        Serializer.Init(AppStates.Languages(zoneId).ToDictionary(l => l.EnvironmentKey.ToLowerInvariant(), l => l.DimensionId),
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
    public virtual XmlExporter Init(int zoneId, int appId, IAppStateInternal appRuntime, bool appExport, string[] attrSetIds, string[] entityIds)
    {
        ContextResolver.SetApp(new AppIdentity(zoneId, appId));
        var ctxOfApp = ContextResolver.App();
        PostContextInit(ctxOfApp);
        Constructor(zoneId, appRuntime, ctxOfApp.AppState.NameId, appExport, attrSetIds, entityIds);

        // this must happen very early, to ensure that the file-lists etc. are correct for exporting when used externally
        InitExportXDocument(ctxOfApp.Site.DefaultCultureCode, EavSystemInfo.VersionString);

        return this;
    }

    /// <summary>
    /// Post context init the caller must be able to init Adam, which is not part of this project, so we're handling it as a callback
    /// </summary>
    /// <param name="appContext"></param>
    protected abstract void PostContextInit(IContextOfApp appContext);

    private void EnsureThisIsInitialized()
    {
        if (Serializer == null || string.IsNullOrEmpty(_appStaticName))
            throw new("Xml Exporter is not initialized - this is required before trying to export");
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

        var dimensions = AppStates.Languages(ZoneId);

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
            if (set.HasPresetAncestor()) continue;

            var attributes = new XElement(XmlConstants.Attributes);

            // Add all Attributes to AttributeSet including meta information
            foreach (var a in set.Attributes.OrderBy(a => a.SortOrder))
            {
                var xmlAttribute = new XElement(XmlConstants.Attribute,
                    new XAttribute(XmlConstants.Static, a.Name),
                    new XAttribute(XmlConstants.Type, a.Type.ToString()),
                    new XAttribute(XmlConstants.IsTitle, a.IsTitle),
                    // Add Attribute MetaData
                    AppState.GetMetadata(TargetTypes.Attribute, a.AttributeId)
                        .Select(c => GetEntityXElement(c.EntityId, c.Type.NameId))
                );

                // #SharedFieldDefinition
                if (a.Guid.HasValue) xmlAttribute.Add(new XAttribute(XmlConstants.Guid, a.Guid));
                if (a.SysSettings != null) xmlAttribute.Add(new XAttribute(XmlConstants.SysSettings, JsonSerializer.Serialize(a.SysSettings, Log)));

                attributes.Add(xmlAttribute);
            }

            // Add AttributeSet / Content Type
            var attributeSet = new XElement(XmlConstants.AttributeSet,
                new XAttribute(XmlConstants.Static, set.NameId),
                new XAttribute(XmlConstants.Name, set.Name),
                new XAttribute(XmlConstants.Scope, set.Scope),
                new XAttribute(XmlConstants.AlwaysShareConfig, set.AlwaysShareConfiguration),
                attributes);

            // Add Ghost-Info if content type inherits from another content type
            if (set.HasAncestor())
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
        if (_isAppExport && _appStaticName != XmlConstants.AppContentGuid && AppState.HasCustomParentApp())
            return new(XmlConstants.ParentApp,
                new XAttribute(XmlConstants.Guid, AppState.ParentAppState.NameId),
                new XAttribute(XmlConstants.AppId, AppState.ParentAppState.AppId)
            );
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

        if (file.RelativePath == null) return null;

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

        if (path != null)
        {
            return new(XmlConstants.Folder,
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