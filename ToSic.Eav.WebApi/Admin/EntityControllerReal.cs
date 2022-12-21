using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Lib.DI;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.Security.Permissions;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.ImportExport;
using ToSic.Eav.WebApi.Plumbing;
using ServiceBase = ToSic.Lib.Services.ServiceBase;

namespace ToSic.Eav.WebApi.Admin
{
    public class EntityControllerReal<THttpResponseType> : ServiceBase, IEntityController<THttpResponseType> 
    {
        public const string LogSuffix = "Entity";
        public EntityControllerReal(
            LazySvc<IContextOfSite> context, 
            LazySvc<IAppStates> appStates, 
            LazySvc<EntityApi> entityApi, 
            LazySvc<ContentExportApi<THttpResponseType>> contentExport, 
            LazySvc<ContentImportApi> contentImport, 
            LazySvc<IUser> user,
            ResponseMaker<THttpResponseType> responseMaker)
            : base("Api.EntityRl") =>
            ConnectServices(
                _context = context,
                _appStates = appStates,
                _entityApi = entityApi,
                _contentExport = contentExport,
                _contentImport = contentImport,
                _user = user,
                _responseMaker = responseMaker
            );

        private readonly LazySvc<IContextOfSite> _context;
        private readonly LazySvc<IAppStates> _appStates;
        private readonly LazySvc<EntityApi> _entityApi;
        private readonly LazySvc<ContentExportApi<THttpResponseType>> _contentExport;
        private readonly LazySvc<ContentImportApi> _contentImport;
        private readonly LazySvc<IUser> _user;
        private readonly ResponseMaker<THttpResponseType> _responseMaker;


        /// <inheritdoc/>
        public IEnumerable<Dictionary<string, object>> List(int appId, string contentType)
            => _entityApi.Value.InitOrThrowBasedOnGrants(_context.Value, _appStates.Value.Get(appId), contentType, GrantSets.ReadSomething)
                .GetEntitiesForAdmin(contentType);


        /// <inheritdoc/>
        public void Delete(string contentType, int appId, int? id, Guid? guid, bool force = false, int? parentId = null,
            string parentField = null)
        {
            if (id.HasValue) _entityApi.Value.InitOrThrowBasedOnGrants(_context.Value, _appStates.Value.Get(appId), contentType, GrantSets.DeleteSomething)
                .Delete(contentType, id.Value, force, parentId, parentField);
            else if (guid.HasValue) _entityApi.Value.InitOrThrowBasedOnGrants(_context.Value, _appStates.Value.Get(appId), contentType, GrantSets.DeleteSomething)
                .Delete(contentType, guid.Value, force, parentId, parentField);
            else
                throw new Exception($"When using '{nameof(Delete)}' you must use 'id' or 'guid' parameters.");
        }


        /// <inheritdoc/>
        public THttpResponseType Json(int appId, int id, string prefix, bool withMetadata)
            => _contentExport.Value.Init(appId).DownloadEntityAsJson(_user.Value, id, prefix, withMetadata);


        /// <inheritdoc/>
        public THttpResponseType Download(
            int appId,
            string language,
            string defaultLanguage,
            string contentType,
            ExportSelection recordExport,
            ExportResourceReferenceMode resourcesReferences,
            ExportLanguageResolution languageReferences, 
            string selectedIds = null)
        {
            var (content, fileName) = _contentExport.Value.Init(appId).ExportContent(
                _user.Value,
                language, defaultLanguage, contentType,
                recordExport, resourcesReferences,
                languageReferences, selectedIds);

            return _responseMaker.File(fileContent: content, fileName: fileName);
        }


        /// <inheritdoc/>
        public ContentImportResultDto XmlPreview(ContentImportArgsDto args)
            => _contentImport.Value.Init(args.AppId).XmlPreview(args);


        /// <inheritdoc/>
        public ContentImportResultDto XmlUpload(ContentImportArgsDto args)
            => _contentImport.Value.Init(args.AppId).XmlImport(args);


        /// <inheritdoc/>
        public bool Upload(EntityImportDto args) => _contentImport.Value.Init(args.AppId).Import(args);


        /// <inheritdoc/>
        //public dynamic Usage(int appId, Guid guid) => _entityBackend.Value.Init(Log).Usage(appId, guid);
    }
}