using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.DI;
using ToSic.Eav.ImportExport.Options;
using ToSic.Lib.Logging;
using ToSic.Eav.Security.Permissions;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.ImportExport;
using ToSic.Eav.WebApi.Plumbing;

namespace ToSic.Eav.WebApi.Admin
{
    public class EntityControllerReal<THttpResponseType> : HasLog, IEntityController<THttpResponseType> 
    {
        public const string LogSuffix = "Entity";
        public EntityControllerReal(
            LazyInitLog<IContextOfSite> context, 
            Lazy<IAppStates> appStates, 
            Lazy<EntityApi> entityApi, 
            Lazy<ContentExportApi<THttpResponseType>> contentExport, 
            Lazy<ContentImportApi> contentImport, 
            Lazy<IUser> user,
            ResponseMaker<THttpResponseType> responseMaker)
            : base("Api.EntityRl")
        {
            _context = context.SetLog(Log);
            _appStates = appStates;
            _entityApi = entityApi;
            _contentExport = contentExport;
            _contentImport = contentImport;
            _user = user;
            _responseMaker = responseMaker;
        }

        private readonly LazyInitLog<IContextOfSite> _context;
        private readonly Lazy<IAppStates> _appStates;
        private readonly Lazy<EntityApi> _entityApi;
        private readonly Lazy<ContentExportApi<THttpResponseType>> _contentExport;
        private readonly Lazy<ContentImportApi> _contentImport;
        private readonly Lazy<IUser> _user;
        private readonly ResponseMaker<THttpResponseType> _responseMaker;


        /// <inheritdoc/>
        public IEnumerable<Dictionary<string, object>> List(int appId, string contentType)
            => _entityApi.Value.InitOrThrowBasedOnGrants(_context.Value, _appStates.Value.Get(appId), contentType, GrantSets.ReadSomething, Log)
                .GetEntitiesForAdmin(contentType);


        /// <inheritdoc/>
        public void Delete(string contentType, int appId, int? id, Guid? guid, bool force = false, int? parentId = null,
            string parentField = null)
        {
            if (id.HasValue) _entityApi.Value.InitOrThrowBasedOnGrants(_context.Value, _appStates.Value.Get(appId), contentType, GrantSets.DeleteSomething, Log)
                .Delete(contentType, id.Value, force, parentId, parentField);
            else if (guid.HasValue) _entityApi.Value.InitOrThrowBasedOnGrants(_context.Value, _appStates.Value.Get(appId), contentType, GrantSets.DeleteSomething, Log)
                .Delete(contentType, guid.Value, force, parentId, parentField);
            else
                throw new Exception($"When using '{nameof(Delete)}' you must use 'id' or 'guid' parameters.");
        }


        /// <inheritdoc/>
        public THttpResponseType Json(int appId, int id, string prefix, bool withMetadata)
            => _contentExport.Value.Init(appId, Log).DownloadEntityAsJson(_user.Value, id, prefix, withMetadata);


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
            var (content, fileName) = _contentExport.Value.Init(appId, Log).ExportContent(
                _user.Value,
                language, defaultLanguage, contentType,
                recordExport, resourcesReferences,
                languageReferences, selectedIds);

            return _responseMaker.File(fileContent: content, fileName: fileName);
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