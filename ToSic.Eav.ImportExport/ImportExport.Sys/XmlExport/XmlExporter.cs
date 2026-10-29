using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Context;
using ToSic.Eav.Data.Sys.Ancestors;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Identity;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.ImportExport.Sys.Xml;
using ToSic.Eav.Sys;

namespace ToSic.Eav.ImportExport.Sys.XmlExport;
// this has a minimal risk of being different!
// should all get it from cache only!

[ShowApiWhenReleased(ShowApiMode.Never)]
public class XmlExporter(XmlSerializer xmlSerializer, IAppsCatalog appsCatalog, IContextOfAppFactory appCtxFactory, IXmlExporterIntegration integration)
    : ServiceWithSetup<XmlExporter.Options>($"{EavLogs.Eav}XmlExp", connect: [appsCatalog, xmlSerializer, appCtxFactory, integration])
{
    public record Options(
        AppExportSpecs Specs,
        IAppReader AppReader,
        bool IsAppExport,
        string[] TypeNamesOrIds,
        string[] EntityIds
    );

    #region simple properties

    public List<TenantFileItem> ReferencedFiles = [];

    public IAppReader AppReader => MyOptions?.AppReader ?? throw new("Xml Exporter is not initialized - this is required before trying to export");

    private string AppStaticName => field ??= AppReader.Specs.NameId; // load late once initialized
    //private AppExportSpecs _specs = null!;
    private List<string> _foldersForEntitiesThatAreNotDeleted = [];
    #endregion

    #region Constructor & DI

    private XmlSerializer Serializer => field ??= SetupSerializerLate();

    private XmlSerializer SetupSerializerLate() => xmlSerializer.Init(
        appsCatalog
            .Zone(MyOptions.Specs.ZoneId)
            .LanguagesActive
            .ToDictionary(
                lng => lng.EnvironmentKey.ToLowerInvariant(),
                lng => lng.Id
            ),
        AppReader);

    /// <summary>
    /// Not that the overload of this must take care of creating the EavAppContext and calling the Constructor
    /// </summary>
    /// <returns></returns>
    public override void Setup(Options options)
    {
        var l = Log.Fn(message: "start XML exporter using app-package");

        var ctxOfApp = appCtxFactory.Create(options.Specs);
        integration.Setup(ctxOfApp);
        base.Setup(options);

        // this must happen very early, to ensure that the file-lists etc. are correct for exporting when used externally
        InitExportXDocument(ctxOfApp.Site.DefaultCultureCode, EavSystemInfo.VersionString);
        l.Done();
    }

    #endregion

    #region Export



    /// <summary>
    /// Exports given Content-Types, Entities and Templates to an XML and returns the XML as string.
    /// </summary>
    /// <returns></returns>
    public string GenerateNiceXml()
    {
        // Will be used to show an export protocol in future
        //Messages = null;

        // Write XDocument to string and return it
        var xmlSettings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            ConformanceLevel = ConformanceLevel.Document,
            Indent = true
        };

        using var stringWriter = new Utf8StringWriter();
        using (var writer = XmlWriter.Create(stringWriter, xmlSettings))
            ExportXDocument.Save(writer);
        return stringWriter.ToString();
    }

    [field: AllowNull, MaybeNull]
    public XDocument ExportXDocument { get; } = XmlBuilder.BuildDocument();

    private void InitExportXDocument(string defaultLanguage, string moduleVersion)
    {
        // Create XML document and declaration
        var doc = ExportXDocument;

        #region Header

        var dimensions = appsCatalog.Zone(MyOptions.Specs.ZoneId).LanguagesActive;

        var header = new XElement(XmlConstants.Header,
            MyOptions.IsAppExport && AppStaticName != XmlConstants.AppContentGuid
                ? new XElement(XmlConstants.App, new XAttribute(XmlConstants.Guid, AppStaticName))
                : null,
            GetParentAppXElement(),
            // Default Language of this site
            new XElement(XmlConstants.Language, new XAttribute(XmlConstants.LangDefault, defaultLanguage)),
            // All languages of this site/export
            new XElement(
                XmlConstants.DimensionDefinition,
                dimensions
                    .Select(d => new XElement(XmlConstants.DimensionDefElement,
                            new XAttribute(XmlConstants.DimId, d.Id),
                            new XAttribute(XmlConstants.Name, d.Name),
                            new XAttribute(XmlConstants.CultureSysKey, d.Key ?? string.Empty),
                            new XAttribute(XmlConstants.CultureExtKey, d.EnvironmentKey ?? string.Empty),
                            new XAttribute(XmlConstants.CultureIsActiveAttrib, d.Active)
                        )
                    )
            )
        );

        #endregion

        #region Attribute Sets

        var contentTypes = new XElement(XmlConstants.AttributeSets);

        // Go through each ContentTypeId
        foreach (var contentTypeId in MyOptions.TypeNamesOrIds)
        {
            var ct = int.TryParse(contentTypeId, out var id)
                ? AppReader.GetContentTypeOptional(id)
                : AppReader.TryGetContentType(contentTypeId);  // in case it's the name, not the number

            // skip system/code-types
            if (ct == null || ct.HasPresetAncestor())
                continue;

            var attributes = new XElement(XmlConstants.Attributes);

            // Add all Attributes to Content-Type/AttributeSet including meta information
            var appMetadata = AppReader.Metadata;
            foreach (var a in ct.Attributes.OrderBy(a => a.SortOrder))
            {
                var xmlAttribute = new XElement(XmlConstants.Attribute,
                    new XAttribute(XmlConstants.Static, a.Name),
                    new XAttribute(XmlConstants.Type, a.Type.ToString()),
                    new XAttribute(XmlConstants.IsTitle, a.IsTitle),
                    // Add Attribute MetaData
                    appMetadata
                        .GetMetadata(TargetTypes.Attribute, a.AttributeId)
                        .Select(c => GetEntityXElement(c.EntityId, c.Type.NameId))
                );

                // #SharedFieldDefinition
                if (a.Guid.HasValue)
                    xmlAttribute.Add(new XAttribute(XmlConstants.Guid, a.Guid));
                if (a.SysSettings != null)
                    xmlAttribute.Add(new XAttribute(XmlConstants.SysSettings, JsonSerializer.Serialize(a.SysSettings, Log)!));

                attributes.Add(xmlAttribute);
            }

            // Add AttributeSet / Content Type
            var contentType = new XElement(XmlConstants.AttributeSet,
                new XAttribute(XmlConstants.Static, ct.NameId),
                new XAttribute(XmlConstants.Name, ct.Name),
                new XAttribute(XmlConstants.Scope, ct.Scope),
                new XAttribute(XmlConstants.AlwaysShareConfig, ct.AlwaysShareConfiguration),
                attributes);

            // Add Ghost-Info if content type inherits from another content type
            if (ct.HasAncestor())
            {
                var parentStaticName = ct.NameId;
                contentType.Add(new XAttribute(XmlConstants.AttributeSetParentDef, parentStaticName));
            }

            contentTypes.Add(contentType);
        }

        #endregion

        #region Entities

        var entities = new XElement(XmlConstants.Entities);

        // Go through each Entity
        foreach (var entityId in MyOptions.EntityIds)
        {
            var id = int.Parse(entityId);

            // Get the entity and ContentType from ContentContext and Add it to ContentItems
            var entity = AppReader.List.FindRepoId(id)
                ?? throw new NullReferenceException($"Tried to find entity {entityId} and it should really exist, but can't find it.");
            var entityXElement = GetEntityXElement(entity.EntityId, entity.Type.NameId);
            entities.Add(entityXElement);
        }

        #endregion

        // init files (add to queue)
        integration.AddFilesToExportQueue();

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
            GetFoldersXElements()
        ));
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
        if (MyOptions.Specs.AssetAdamDeleted)
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

    private XElement? GetParentAppXElement()
    {
        if (!MyOptions.IsAppExport || AppStaticName == XmlConstants.AppContentGuid || !AppReader.HasCustomParentApp())
            return null;

        var parentAppState = AppReader.GetParentCache()!;
        return new(XmlConstants.ParentApp,
            new XAttribute(XmlConstants.Guid, parentAppState.NameId),
            new XAttribute(XmlConstants.AppId, parentAppState.AppId)
        );
    }


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
            //var valueKey = value.Attribute(XmlConstants.KeyAttr)?.Value;

            if (string.IsNullOrEmpty(valueString))
                continue;

            if (valueType != XmlConstants.ValueTypeLink)
                continue;

            // Collect all referenced files for adding a file list to the xml later
            var fileRegex = new Regex(XmlConstants.FileRefRegex, RegexOptions.IgnoreCase);
            var a = fileRegex.Match(valueString);
            // try to remember the file
            if (a.Success && a.Groups[XmlConstants.FileIdInRegEx].Length > 0)
                integration.AddFileAndFolderToQueue(int.Parse(a.Groups[XmlConstants.FileIdInRegEx].Value));
        }

        return entityXElement;
    }

    #endregion


    #region Files & Pages

    private XElement GetFilesXElements() => new(XmlConstants.PortalFiles, integration.ReferencedFileIds.Distinct().Select(GetFileXElement));


    private XElement GetFoldersXElements() => new(XmlConstants.FolderGroup, integration.ReferencedFolderIds.Distinct().Select(GetFolderXElement));

    private XElement? GetFileXElement(int fileId)
    {
        var file = integration.ResolveFile(fileId);

        if (IsDeletedOrInvalid(file?.RelativePath))
            return null;

        file!.RelativePath = file.RelativePath!.ForwardSlash();
        ReferencedFiles.Add(file!);

        return new(XmlConstants.FileNode,
            new XAttribute(XmlConstants.FileIdAttr, file!.Id),
            new XAttribute(XmlConstants.FolderNodePath, file.RelativePath!)
        );
    }

    private XElement? GetFolderXElement(int folderId)
    {
        var path = integration.ResolveFolderId(folderId).ForwardSlash();

        if (IsDeletedOrInvalid(path))
            return null;

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
    private bool IsDeletedOrInvalid(string? relativePath)
    {
        // invalid, can't export assets without path
        if (relativePath == null)
            return true;

        // skip check for deleted assets and just leave it for export
        if (MyOptions.Specs.AssetAdamDeleted)
            return false;

        // if not in "adam" folder, we can't know is it deleted or not, so leave it for export
        // this is expected for site assets
        if (!relativePath.StartsWith("adam"))
            return false;

        // break adam path to parts,
        // this is necessary to check if it's in discriminator list of possible folders for entities that are not deleted
        var pathParts = relativePath.ForwardSlash().Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        // always export parent folders for entity assets (like adam/app/)
        if (pathParts.Length < 3)
            return false;

        // check if folder is in list of possible entities that are not deleted
        return _foldersForEntitiesThatAreNotDeleted.All(f => f != pathParts[2]);
    }
    #endregion

    /// <summary>
    /// StringWriter with UTF8 Encoding
    /// </summary>
    private class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}