using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
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
        private readonly LazySvc<AppWork> _appSys;

        #region Constructor / DI

        public ContentTypeApi(
            LazySvc<AppWork> appSys, 
            LazySvc<AppManager> appManagerLazy, 
            LazySvc<DbDataController> dbLazy, 
            AppInitializedChecker appInitializedChecker,
            LazySvc<IConvertToEavLight> convertToEavLight, 
            LazySvc<DataBuilder> multiBuilder,
            IAppStates appStates) : base("Api.EavCTC")
        {
            ConnectServices(
                _appSys = appSys,
                _appManagerLazy = appManagerLazy,
                _dbLazy = dbLazy,
                _appInitializedChecker = appInitializedChecker,
                _convertToEavLight = convertToEavLight,
                _multiBuilder = multiBuilder,
                _appStates = appStates
            );
        }

        private readonly LazySvc<DataBuilder> _multiBuilder;
        private readonly LazySvc<AppManager> _appManagerLazy;
        private readonly LazySvc<DbDataController> _dbLazy;
        private readonly AppInitializedChecker _appInitializedChecker;
        private readonly LazySvc<IConvertToEavLight> _convertToEavLight;
        private readonly IAppStates _appStates;
        private AppManager AppManager { get; set; }

        public ContentTypeApi Init(int appId)
        {
            var l = Log.Fn<ContentTypeApi>($"{appId}");
            _appId = appId;
            _appCtxPlus = _appSys.Value.ContextPlus(appId);
            AppManager = _appManagerLazy.Value.Init(appId);
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
            var appEntities = _appSys.Value.Entities;

            // get all types
            var allTypes = _appCtxPlus.AppState.ContentTypes.OfScope(scope, true);

            var filteredType = allTypes.Where(t => t.Scope == scope)
                .OrderBy(t => t.Name)
                .Select(t => ContentTypeAsDto(t, appEntities.Get(_appCtxPlus, t.Name).Count()));
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

	    public bool Delete(string staticName)
	    {
	        var l = Log.Fn<bool>($"delete a#{_appId}, name:{staticName}");
            GetDb().ContentType.Delete(staticName);
	        return l.ReturnTrue();
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

        public bool CreateGhost(string sourceStaticName)
	    {
	        var l = Log.Fn<bool>($"create ghost a#{_appId}, type:{sourceStaticName}");
	        GetDb().ContentType.CreateGhost(sourceStaticName);
            return l.ReturnTrue();
	    }

        #region Fields - Get, Reorder, Data-Types (for dropdown), etc.

        /// <summary>
        /// Returns the configuration for a content type
        /// </summary>
        public IEnumerable<ContentTypeFieldDto> GetFields(string staticName)
        {
            var l = Log.Fn<IEnumerable<ContentTypeFieldDto>>($"get fields a#{_appId}, type:{staticName}");
            var appState = _appStates.Get(_appId);

            if (!(appState.GetContentType(staticName) is IContentType type))
                return l.Return(new List<ContentTypeFieldDto>(),
                    $"error, type:{staticName} is null, it is missing or it is not a ContentType - something broke");

            var fields = type.Attributes.OrderBy(a => a.SortOrder);

            return l.Return(fields.Select(a => FieldAsDto(a, type, false)));
        }

        /// <summary>
        /// TODO: implemented, but never tested yet, WIP with @SDV
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ContentTypeFieldDto> GetSharedFields()
        {
            var l = Log.Fn<IEnumerable<ContentTypeFieldDto>>($"get shared fields a#{_appId}");
            var appState = _appStates.Get(_appId);
            var localTypes = appState.ContentTypes
                .Where(ct => !ct.HasAncestor())
                .ToList();

            var fields = localTypes
                .SelectMany(ct => ct.Attributes
                    .Where(a => a.Guid != null && a.SysSettings?.Share == true)
                    .Select(a => new { Type = ct, Field = a}))
                .OrderBy(set => set.Type.Name)
                .ThenBy(set => set.Field.Name)
                .ToList();

            return l.Return(fields.Select(a => FieldAsDto(a.Field, a.Type, true)));
        }

        private List<InputTypeInfo> AppInputTypes => _appInputTypes.Get(() => _appSys.Value.InputTypes.GetInputTypes(_appCtxPlus));
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
                HasFormulas = HasCalculations(a),

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

        private bool HasCalculations(IContentTypeAttribute attribute)
        {
            var l = Log.Fn<bool>(attribute.Name);
            var allMd = attribute.Metadata.FirstOrDefaultOfType(AttributeMetadata.TypeGeneral);
            if (allMd == null) return l.ReturnFalse("no @All");

            var calculationsAttr = allMd.Attributes.Values.FirstOrDefault(a => a.Name == AttributeMetadata.MetadataFieldAllFormulas);
            if (calculationsAttr == null) return l.ReturnFalse("no calc property");

            var calculations = calculationsAttr.Values?.FirstOrDefault()?.ObjectContents as IEnumerable<IEntity>;
            return l.Return(calculations?.Any() ?? false);
        }


        public bool Reorder(int contentTypeId, string newSortOrder)
        {
            var l = Log.Fn<bool>($"reorder type#{contentTypeId}, order:{newSortOrder}");
            var sortOrderList = newSortOrder.Trim('[', ']').Split(',').Select(int.Parse).ToList();
            GetDb().ContentType.SortAttributes(contentTypeId, sortOrderList);
            return l.ReturnTrue();
        }

	    public string[] DataTypes()
	    {
	        Log.A($"get data types");
            return GetDb().Attributes.DataTypeNames();
	    }


        public int AddField(int contentTypeId, string staticName, string type, string inputType, int sortOrder)
	    {
	        Log.A($"add field type#{contentTypeId}, name:{staticName}, type:{type}, input:{inputType}, order:{sortOrder}");
            var attDef = _multiBuilder.Value.TypeAttributeBuilder
                .Create(appId: AppManager.AppId, name: staticName, type: ValueTypeHelpers.Get(type), isTitle: false, id: 0, sortOrder: sortOrder);
            return AppManager.ContentTypes.CreateAttributeAndInitializeAndSave(contentTypeId, attDef, inputType);
	    }

        public bool SetInputType(int attributeId, string inputType)
        {
            Log.A($"update input type attrib:{attributeId}, input:{inputType}");
            return AppManager.ContentTypes.UpdateInputType(attributeId, inputType);
        }

	    public bool DeleteField(int contentTypeId, int attributeId)
	    {
	        var l = Log.Fn<bool>($"delete field type#{contentTypeId}, attrib:{attributeId}");
            return l.Return(GetDb().Attributes.RemoveAttributeAndAllValuesAndSave(attributeId));
	    }

	    public void SetTitle(int contentTypeId, int attributeId)
	    {
	        var l = Log.Fn($"set title type#{contentTypeId}, attrib:{attributeId}");
	        GetDb().Attributes.SetTitleAttribute(attributeId, contentTypeId);
            l.Done();
        }

        public bool Rename(int contentTypeId, int attributeId, string newName)
        {
            var l = Log.Fn<bool>($"rename attribute type#{contentTypeId}, attrib:{attributeId}, name:{newName}");
            GetDb().Attributes.RenameAttribute(attributeId, contentTypeId, newName);
            return l.ReturnTrue();
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