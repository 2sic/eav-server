﻿using ToSic.Eav.Apps.Sys.Paths;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.ImportExport.Json.Sys;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.ImportExport.Sys.Options;
using ToSic.Eav.ImportExport.Sys.XmlList;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Security.Files;
using ToSic.Eav.Serialization.Sys;
using ToSic.Eav.Sys;
using ToSic.Eav.WebApi.Sys.Helpers.Http;
using ToSic.Eav.WebApi.Sys.Security;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Users;
#if NETFRAMEWORK
using System.Web.Http;
#else
using Microsoft.AspNetCore.Mvc;
#endif
#if NETFRAMEWORK
using THttpResponseType = System.Net.Http.HttpResponseMessage;
#else
using THttpResponseType = Microsoft.AspNetCore.Mvc.IActionResult;
#endif


namespace ToSic.Eav.WebApi.Sys.ImportExport;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContentExportApi(
    IAppPathsMicroSvc appPathSvc,
    AppWorkContextService appWorkCtxSvc,
    IAppsCatalog appsCatalog,
    Generator<JsonSerializer> jsonSerializer,
    IResponseMaker responseMaker,
    Generator<ExportListXml> exportListXmlGenerator,
    LazySvc<ISysFeaturesService> features)
    : ServiceBase("Api.EaCtEx",
        connect: [appPathSvc, appWorkCtxSvc, exportListXmlGenerator, appsCatalog, jsonSerializer, responseMaker, features])
{
    public ContentExportApi Init(int appId)
    {
        var l = Log.Fn<ContentExportApi>($"For app: {appId}");
        _appCtx = appWorkCtxSvc.Context(appId);
        return l.Return(this);
    }

    private IAppWorkCtx _appCtx = null!;

    public (string FileContents, string FileName) ExportContent(IUser user,
        string language,
        string defaultLanguage,
        string contentType,
        ExportSelection exportSelection,
        ExportResourceReferenceMode exportResourcesReferences,
        ExportLanguageResolution exportLanguageReferences,
        string? selectedIds)
    {
        var l = Log.Fn<(string, string)>($"export content lang:{language}, deflang:{defaultLanguage}, ct:{contentType}, ids:{selectedIds}");
        SecurityHelpers.ThrowIfNotContentAdmin(user, l);

        var contextLanguages = appsCatalog.Zone(_appCtx.ZoneId).LanguagesActive
            .Select(lng => lng.EnvironmentKey)
            .ToArray();

        // check if we have an array of ids
        int[]? ids = null;
        try
        {
            if (exportSelection == ExportSelection.Selection && !string.IsNullOrWhiteSpace(selectedIds))
                ids = selectedIds!.Split(',').Select(int.Parse).ToArray();
        }
        catch (Exception e)
        {
            throw new("trouble finding selected IDs to export", e);
        }

        var tableExporter = exportListXmlGenerator.New().Init(_appCtx.AppReader, contentType);
        var fileContent = exportSelection == ExportSelection.Blank
            ? tableExporter.EmptyListTemplate()!
            : tableExporter.GenerateXml(language ?? "", defaultLanguage, contextLanguages, exportLanguageReferences,
                exportResourcesReferences == ExportResourceReferenceMode.Resolve, ids!)!;

        var contentTypeName = tableExporter.ContentType.Name;

        var fileName =
            $"2sxc {contentTypeName.Replace(" ", "-")} {language} " +
            $"{(exportSelection == ExportSelection.Blank ? "Template" : "Data")} " +
            $"{DateTime.Now:yyyyMMddHHmmss}.xml";

        return l.ReturnAsOk((fileContent, fileName));
    }

    [HttpGet]
    public THttpResponseType DownloadTypeAsJson(IUser user, string name)
    {
        var l = Log.Fn<THttpResponseType>($"get fields type:{name}");
        SecurityHelpers.ThrowIfNotSiteAdmin(user, l);
        var type = _appCtx.AppReader.GetContentType(name);
        var serializer = jsonSerializer.New().SetApp(_appCtx.AppReader);
        var fileName = (type.Scope + "." + type.NameId + ImpExpConstants.Extension(ImpExpConstants.Files.json))
            .RemoveNonFilenameCharacters();

        var typeJson = serializer.Serialize(type, new()
        {
            CtIncludeInherited = false,
            CtAttributeIncludeInheritedMetadata = false
        });
        return l.ReturnAsOk(responseMaker.File(typeJson, fileName, MimeTypeConstants.Json));
    }

    [HttpGet]
    public THttpResponseType DownloadEntityAsJson(IUser user, int id, string prefix, bool withMetadata)
    {
        var l = Log.Fn<THttpResponseType>($"get fields id:{id}");
        SecurityHelpers.ThrowIfNotSiteAdmin(user, l);
        var entity = _appCtx.AppReader.List.FindRepoId(id)
            ?? throw new($"Can't find entity with id {id}");
        var serializer = jsonSerializer.New().SetApp(_appCtx.AppReader);

        return l.ReturnAsOk(responseMaker.File(
            serializer.Serialize(entity, withMetadata ? FileSystemLoaderConstants.QueryMetadataDepth : 0),
            (prefix + (string.IsNullOrWhiteSpace(prefix) ? "" : ".")
             + entity.GetBestTitle() + ImpExpConstants.Extension(ImpExpConstants.Files.json))
            .RemoveNonFilenameCharacters()));
    }

    [HttpGet]
    public THttpResponseType JsonBundleExport(IUser user, Guid exportConfiguration, int indentation)
    {
        var l = Log.Fn<THttpResponseType>($"create Json Bundle Export for ExportConfiguration:{exportConfiguration}");
        SecurityHelpers.ThrowIfNotSiteAdmin(user, l);

        features.Value.ThrowIfNotEnabled("This feature is required", BuiltInFeatures.DataExportImportBundles.Guid);

        var (export, fileContent) = CreateBundleExport(exportConfiguration, indentation);

        return l.ReturnAsOk(responseMaker.File(fileContent, export.FileName, MimeTypeConstants.Json));
    }

    public bool BundleSave(IUser user, Guid exportConfiguration, int indentation)
    {
        var l = Log.Fn<bool>($"for ExportConfiguration:{exportConfiguration}");

        SecurityHelpers.ThrowIfNotSiteAdmin(user, l);

        features.Value.ThrowIfNotEnabled("This feature is required", BuiltInFeatures.DataExportImportBundles.Guid);

        var (export, fileContent) = CreateBundleExport(exportConfiguration, indentation);

        var appPaths = appPathSvc.Get(_appCtx.AppReader);

        var appDataPath = Path.Combine(appPaths.PhysicalPath, FolderConstants.DataFolderProtected, AppDataFoldersConstants.BundlesFolder);
        l.A($"appDataPath:'{appDataPath}'");

        try
        {
            // create App_Data\bundles unless exists
            Directory.CreateDirectory(appDataPath);

            var fileNameSafe = FileNames.SanitizeFileName(export.FileName);
            if (export.FileName != fileNameSafe) l.A($"File name sanitized:'{export.FileName}' => '{fileNameSafe}'");

            var filePath = Path.Combine(appDataPath, fileNameSafe);

            // save bundle file to App_Data\bundles
            File.WriteAllText(filePath, fileContent);
            l.A($"bundle saved, filePath:'{filePath}',size:{fileContent.Length}");

            return l.ReturnTrue();
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            return l.ReturnFalse();
        }
    }

    private (ExportConfiguration export, string fileContent) CreateBundleExport(Guid exportConfiguration, int indentation)
    {
        var l = Log.Fn<(ExportConfiguration export, string fileContent)>($"create bundle export for ExportConfiguration:{exportConfiguration}");

        var export = ExportConfigurationBuildOrThrow(exportConfiguration);

        // find all decorator metadata of type SystemExportDecorator
        l.A($"metadataExportMarkers:{export.ExportMarkers.Count}");

        var serializer = jsonSerializer.New().SetApp(_appCtx.AppReader);

        var bundleBuilder = new JsonBundleBuilder(_appCtx.AppReader, Log);
        var bundle = bundleBuilder.BundleBuild(export, serializer);

        // create a file which contains this new bundle
        var fileContent = serializer.SerializeJsonBundle(bundle, indentation);

        // give it to the browser with the name specified in the Export Configuration
        l.A($"OK, export fileName:{export.FileName}, size:{fileContent.Length}");

        return l.ReturnAsOk((export, fileContent));
    }

    private ExportConfiguration ExportConfigurationBuildOrThrow(Guid exportConfiguration)
    {
        var l = Log.Fn<ExportConfiguration>($"build ExportConfiguration:{exportConfiguration}");
        var systemExportConfiguration = _appCtx.AppReader.List.One(exportConfiguration);
        if (systemExportConfiguration == null)
        {
            var exception = new KeyNotFoundException($"ExportConfiguration:{exportConfiguration} is missing");
            l.Ex(exception);
            throw exception;
        }

        // check that have correct contentType
        if (systemExportConfiguration.Type.Is("ExportConfiguration"))
        {
            var exception =
                new KeyNotFoundException($"ExportConfiguration:{exportConfiguration} is not of type ExportConfiguration");
            l.Ex(exception);
            throw exception;
        }
        
        return l.ReturnAsOk(new(systemExportConfiguration));
    }

}
