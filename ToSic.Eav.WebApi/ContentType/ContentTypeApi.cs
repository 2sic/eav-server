using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Lib.Logging;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Serialization;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.Security;
using ToSic.Lib.DI;
using ToSic.Lib.Helpers;
using ToSic.Lib.Services;
using ToSic.Eav.Apps.Work;

namespace ToSic.Eav.WebApi
{
    /// <inheritdoc />
    /// <summary>
    /// Web API Controller for ContentTypes
    /// </summary>
    public partial class ContentTypeApi : ServiceBase
    {
        #region Constructor / DI

        public ContentTypeApi(
            LazySvc<DbDataController> dbLazy, 
            AppInitializedChecker appInitializedChecker,
            LazySvc<IConvertToEavLight> convertToEavLight,
            GenWorkPlus<WorkEntities> workEntities,
            GenWorkPlus<WorkInputTypes> inputTypes,
            GenWorkBasic<WorkAttributes> attributes,
            IAppStates appStates) : base("Api.EavCTC")
        {
            ConnectServices(
                _inputTypes = inputTypes,
                _attributes = attributes,
                _dbLazy = dbLazy,
                _appInitializedChecker = appInitializedChecker,
                _convertToEavLight = convertToEavLight,
                _workEntities = workEntities,
                _appStates = appStates
            );
        }


        private readonly GenWorkBasic<WorkAttributes> _attributes;
        private readonly GenWorkPlus<WorkInputTypes> _inputTypes;
        private readonly GenWorkPlus<WorkEntities> _workEntities;
        private readonly LazySvc<DbDataController> _dbLazy;
        private readonly AppInitializedChecker _appInitializedChecker;
        private readonly LazySvc<IConvertToEavLight> _convertToEavLight;
        private readonly IAppStates _appStates;

        public ContentTypeApi Init(int appId)
        {
            var l = Log.Fn<ContentTypeApi>($"{appId}");
            _appId = appId;
            _appCtxPlus = _workEntities.CtxSvc.ContextPlus(appId);
            return l.Return(this);
        }

        private int _appId;
        private IAppWorkCtxPlus _appCtxPlus;

        #endregion

        #region Content-Type Get, Delete, Save

        public IEnumerable<ContentTypeDto> List(string scope = null, bool withStatistics = false)
        {
            var l = Log.Fn<IEnumerable<ContentTypeDto>>($"scope:{scope}, stats:{withStatistics}");

            // 2020-01-15 2sxc 10.27.00 Special side-effect, pre-generate the resources, settings etc. if they didn't exist yet
            // this is important on "Content" apps, because these don't auto-initialize when loading from the DB
            // so for these, we must pre-ensure that the app is initialized as needed, if they 
            // are editing the resources etc. 
            if (scope == Data.Scopes.App)
            {
                l.A($"is scope {scope}, will do extra processing");
                // make sure additional settings etc. exist
                _appInitializedChecker.EnsureAppConfiguredAndInformIfRefreshNeeded(_appCtxPlus.AppState, null, Log); 
            }
            // should use app-manager and return each type 1x only
            var appEntities = _workEntities.New(_appCtxPlus);

            // get all types
            var allTypes = _appCtxPlus.AppState.ContentTypes.OfScope(scope, true);

            var filteredType = allTypes.Where(t => t.Scope == scope)
                .OrderBy(t => t.Name)
                .Select(t => ContentTypeAsDto(t, appEntities.Get(t.Name).Count()));
            return l.ReturnAsOk(filteredType);
	    }

        private ContentTypeDto ContentTypeAsDto(IContentType t, int count = -1)
	    {
	        var l = Log.Fn<ContentTypeDto>($"for json a:{t.AppId}, type:{t.Name}");
	        var details = t.Metadata.DetailsOrNull;

            var nameOverride = details?.Title;
            if (string.IsNullOrEmpty(nameOverride))
	            nameOverride = t.Name;
            var ser = _convertToEavLight.Value;

            var ancestorDecorator = t.GetDecorator<IAncestor>();

            var properties = ser.Convert(details?.Entity);

            var jsonReady = new ContentTypeDto
            {
                Id = t.Id,
                Name = t.Name,
                Label = nameOverride,
                StaticName = t.NameId,
                Scope = t.Scope,
                Description = details?.Description,
                EditInfo = new EditInfoDto(t),
                UsesSharedDef = ancestorDecorator != null,
                SharedDefId = ancestorDecorator?.Id,
                Items = count,
                Fields = t.Attributes.Count(),
                Metadata = (ser as ConvertToEavLight)?.CreateListOfSubEntities(t.Metadata,
                    SubEntitySerialization.AllTrue()),
                Properties = properties,
                Permissions = new HasPermissionsDto { Count = t.Metadata.Permissions.Count() },
            };
	        return l.ReturnAsOk(jsonReady);
	    }

	    public ContentTypeDto GetSingle(string contentTypeStaticName, string scope = null)
	    {
	        var l = Log.Fn<ContentTypeDto>($"a#{_appId}, type:{contentTypeStaticName}, scope:{scope}");
            var appState = _appStates.Get(_appId);
            var ct = appState.GetContentType(contentTypeStaticName);
            return l.Return(ContentTypeAsDto(ct));
	    }

        public bool Save(Dictionary<string, string> item)
	    {
	        var l = Log.Fn<bool>($"save a#{_appId}, item count:{item?.Count}");
	        if (item == null)
                return l.ReturnFalse("item was null, will cancel");

            GetDb().ContentType.AddOrUpdate(
                item["StaticName"], 
                item["Scope"], 
                item["Name"],
                null, false);
            return l.ReturnTrue();
        }
        #endregion

        #region Fields - Get, Reorder, Data-Types (for dropdown), etc.

        /// <summary>
        /// Returns the configuration for a content type
        /// </summary>
        public IEnumerable<ContentTypeFieldDto> GetFields(string staticName)
        {
            var l = Log.Fn<IEnumerable<ContentTypeFieldDto>>($"get fields a#{_appId}, type:{staticName}");
            
            var fields = _attributes.New(_appCtxPlus).GetFields(staticName);

            return l.Return(fields.Select(a => FieldAsDto(a.Field, a.Type, false)));
        }

        /// <summary>
        /// TODO: implemented, but never tested yet, WIP with @SDV
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ContentTypeFieldDto> GetSharedFields()
        {
            var l = Log.Fn<IEnumerable<ContentTypeFieldDto>>($"get shared fields a#{_appId}");

            var fields = _attributes.New(_appId).GetSharedFields();

            return l.Return(fields.Select(a => FieldAsDto(a.Field, a.Type, true)));
        }

        private List<InputTypeInfo> AppInputTypes => _appInputTypes.Get(() => _inputTypes.New(_appCtxPlus).GetInputTypes());
        private readonly GetOnce<List<InputTypeInfo>> _appInputTypes = new GetOnce<List<InputTypeInfo>>();

        private ContentTypeFieldDto FieldAsDto(IContentTypeAttribute a, IContentType type, bool withContentType)
        {
            var ancestorDecorator = type.GetDecorator<IAncestor>();
            var inputType = FindInputType(a);
            var ser = _convertToEavLight.Value;
            return new ContentTypeFieldDto
            {
                Id = a.AttributeId,
                SortOrder = a.SortOrder,
                Type = a.Type.ToString(),
                InputType = inputType,
                StaticName = a.Name,
                IsTitle = a.IsTitle,
                AttributeId = a.AttributeId,
                Metadata = a.Metadata
                    .ToDictionary(
                        e =>
                        {
                            // if the static name is a GUID, then use the normal name as name-giver
                            var name = Guid.TryParse(e.Type.NameId, out _)
                                ? e.Type.Name
                                : e.Type.NameId;
                            return name.TrimStart('@');
                        },
                        e => InputMetadata(type, a, e, ancestorDecorator, ser)),
                InputTypeConfig = AppInputTypes.FirstOrDefault(it => it.Type == inputType),
                Permissions = new HasPermissionsDto { Count = a.Metadata.Permissions.Count() },

                // new in 12.01
                IsEphemeral = a.Metadata.GetBestValue<bool>(AttributeMetadata.MetadataFieldAllIsEphemeral,
                    AttributeMetadata.TypeGeneral),
                HasFormulas = a.HasFormulas(Log),

                // Read-Only new in v13
                EditInfo = new EditInfoAttributeDto(type, a),

                // #SharedFieldDefinition
                Guid = a.Guid,
                SysSettings = JsonAttributeSysSettings.FromSysSettings(a.SysSettings),
                ContentType = withContentType ? new JsonType(type, false, false) : null,
            };
        }

        private EavLightEntity InputMetadata(IContentType contentType, IContentTypeAttribute a, IEntity e, IAncestor ancestor, IConvertToEavLight ser)
        {
            var result = ser.Convert(e);
            if (ancestor != null)
                result.Add("IdHeader", new
                {
                    e.EntityId,
                    Ancestor = true,
                    IsMetadata = true,
                    OfContentType = contentType.NameId,
                    OfAttribute = a.Name,
                });

            return result;
        }

        /// <summary>
        /// The old method, which returns the text "unknown" if not known. 
        /// As soon as the new UI is used, this must be removed / deprecated
        /// TODO: 2023-11 @2dm - this seems very old and has a note that it should be removed on new UI, but I'm not sure if this has already happened
        /// TODO: should probably check if the UI still does any "unknown" checks
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// It's important to NOT cache this result, because it can change during runtime, and then a cached info would be wrong. 
        /// </remarks>
        private static string FindInputType(IContentTypeAttribute attribute)
        {
            var inputType = attribute.Metadata.GetBestValue<string>(AttributeMetadata.GeneralFieldInputType, AttributeMetadata.TypeGeneral);

            // unknown will let the UI fallback on other mechanisms
            return string.IsNullOrEmpty(inputType) ? Constants.NullNameId : inputType;
        }

        #endregion

        #region New Sharing Features

        public void FieldShare(int attributeId, bool share, bool hide = false)
        {
            // TODO: @STV
            // - Ensure GUID: update the field definition in the DB to ensure it has a GUID (but don't change if it already has one)
            // - get the fields current SysSettings and update with the Share = share (hide we'll ignore for now, it's for future needs)
            // - Update DB
            // - Then flush the app-cache as necessary, same as any other attribute change
        }

        public void FieldInherit(int attributeId, Guid inheritMetadataOf)
        {
            // TODO: @STV
            // - Get field attributeId and it's sys-settings
            // - set InheritMetadataOf to the guid above (as string)
            // - save
            // - flush app-cache as necessary, same as any other attribute change
        }

        #endregion

        private DbDataController GetDb() => _db ?? (_db = _dbLazy.Value.Init(null, _appId));
        private DbDataController _db;
    }


}