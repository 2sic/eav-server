using System;
using System.Collections.Generic;
using System.Net.Http;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Security.Permissions;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.ImportExport;

namespace ToSic.Eav.WebApi.Admin
{
    public class EntityControllerReal : HasLog<EntityControllerReal>, IEntityController 
    {
        public const string LogSuffix = "Entity";
        public EntityControllerReal(LazyInitLog<IContextOfSite> context, Lazy<IAppStates> appStates, Lazy<EntityApi> entityApi, Lazy<ContentExportApi> contentExport, Lazy<ContentImportApi> contentImport, Lazy<IUser> user)
            : base("Api.EntityRl")
        {
            _context = context.SetLog(Log);
            _appStates = appStates;
            _entityApi = entityApi;
            _contentExport = contentExport;
            _contentImport = contentImport;
            _user = user;
        }

        private readonly LazyInitLog<IContextOfSite> _context;
        private readonly Lazy<IAppStates> _appStates;
        private readonly Lazy<EntityApi> _entityApi;
        private readonly Lazy<ContentExportApi> _contentExport;
        private readonly Lazy<ContentImportApi> _contentImport;
        private readonly Lazy<IUser> _user;


        /// <inheritdoc/>
        public IEnumerable<Dictionary<string, object>> List(int appId, string contentType)
            => _entityApi.Value.InitOrThrowBasedOnGrants(_context.Ready, _appStates.Value.Get(appId), contentType, GrantSets.ReadSomething, Log)
                .GetEntitiesForAdmin(contentType);


        /// <inheritdoc/>
        public void Delete(string contentType, int? id, Guid? guid, int appId, bool force = false, int? parentId = null, string parentField = null)
        {
            if (id.HasValue) _entityApi.Value.InitOrThrowBasedOnGrants(_context.Ready, _appStates.Value.Get(appId), contentType, GrantSets.DeleteSomething, Log)
                .Delete(contentType, id.Value, force, parentId, parentField);
            else if (guid.HasValue) _entityApi.Value.InitOrThrowBasedOnGrants(_context.Ready, _appStates.Value.Get(appId), contentType, GrantSets.DeleteSomething, Log)
                .Delete(contentType, guid.Value, force, parentId, parentField);
            throw new Exception($"When using '{nameof(Delete)}' you must use 'id' or 'guid' parameters.");
        }


        /// <inheritdoc/>
        public HttpResponseMessage Json(int appId, int id, string prefix, bool withMetadata)
            => _contentExport.Value.Init(appId, Log).DownloadEntityAsJson(_user.Value, id, prefix, withMetadata);


        /// <inheritdoc/>
        public HttpResponseMessage Download(
            int appId,
            string language,
            string defaultLanguage,
            string contentType,
            ExportSelection recordExport,
            ExportResourceReferenceMode resourcesReferences,
            ExportLanguageResolution languageReferences, 
            string selectedIds = null)
        {
            var (content, fileName) = _contentExport.Value.Init(appId, Log).ExportContent(
                _user.Value,
                language, defaultLanguage, contentType,
                recordExport, resourcesReferences,
                languageReferences, selectedIds);

            return Eav.WebApi.Helpers.Download.BuildDownload(
                content: content,
                fileName: fileName);
        }


        /// <inheritdoc/>
        public ContentImportResultDto XmlPreview(ContentImportArgsDto args)
            => _contentImport.Value.Init(args.AppId, Log).XmlPreview(args);


        /// <inheritdoc/>
        public ContentImportResultDto XmlUpload(ContentImportArgsDto args)
            => _contentImport.Value.Init(args.AppId, Log).XmlImport(args);


        /// <inheritdoc/>
        public bool Upload(EntityImportDto args) => _contentImport.Value.Init(args.AppId, Log).Import(args);


        /// <inheritdoc/>
        //public dynamic Usage(int appId, Guid guid) => _entityBackend.Value.Init(Log).Usage(appId, guid);
    }
}