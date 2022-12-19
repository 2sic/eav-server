using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Lib.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Serialization;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.Security;

namespace ToSic.Eav.WebApi
{
	/// <inheritdoc />
	/// <summary>
	/// Web API Controller for ContentTypes
	/// </summary>
	public partial class ContentTypeApi : HasLog
    {
        #region Constructor / DI

        public ContentTypeApi(
            Lazy<AppRuntime> appRuntimeLazy, 
            Lazy<AppManager> appManagerLazy, 
            Lazy<DbDataController> dbLazy, 
            AppInitializedChecker appInitializedChecker,
            Lazy<IConvertToEavLight> convertToEavLight, 
            IAppStates appStates) : base("Api.EavCTC")
        {
            _appRuntimeLazy = appRuntimeLazy;
            _appManagerLazy = appManagerLazy;
            _dbLazy = dbLazy;
            _appInitializedChecker = appInitializedChecker;
            _convertToEavLight = convertToEavLight;
            _appStates = appStates;
        }

        private readonly Lazy<AppRuntime> _appRuntimeLazy;
        private readonly Lazy<AppManager> _appManagerLazy;
        private readonly Lazy<DbDataController> _dbLazy;
        private readonly AppInitializedChecker _appInitializedChecker;
        private readonly Lazy<IConvertToEavLight> _convertToEavLight;
        private readonly IAppStates _appStates;
        private AppManager AppManager { get; set; }

        public ContentTypeApi Init(int appId, ILog parentLog)
        {
            this.Init(parentLog);
            Log.A($"Will use app {appId}");
            _appId = appId;
            AppManager = _appManagerLazy.Value.Init(Log).Init(appId);
            return this;
        }

        private int _appId;

        #endregion

        #region Content-Type Get, Delete, Save

        // todo: rename to "List" to match external name, once feature/oqtane2 branch isn't used any more
        public IEnumerable<ContentTypeDto> Get(string scope = null, bool withStatistics = false)
        {
            var wrapLog = Log.Fn<IEnumerable<ContentTypeDto>>($"scope:{scope}, stats:{withStatistics}");

            // 2020-01-15 2sxc 10.27.00 Special side-effect, pre-generate the resources, settings etc. if they didn't exist yet
            // this is important on "Content" apps, because these don't auto-initialize when loading from the DB
            // so for these, we must pre-ensure that the app is initialized as needed, if they 
            // are editing the resources etc. 
            if (scope == Data.Scopes.App)
            {
                Log.A($"is scope {scope}, will do extra processing");
                var appState = _appStates.Get(AppManager);
                // make sure additional settings etc. exist
                _appInitializedChecker.EnsureAppConfiguredAndInformIfRefreshNeeded(appState, null, Log); 
            }
            // should use app-manager and return each type 1x only
            var appMan = AppManager;

            // get all types
            var allTypes = appMan.Read.ContentTypes.All.OfScope(scope, true);

            var filteredType = allTypes.Where(t => t.Scope == scope)
                .OrderBy(t => t.Name)
                .Select(t => ContentTypeAsDto(t, appMan.Read.Entities.Get(t.Name).Count()));
            return wrapLog.ReturnAsOk(filteredType);
	    }

        private ContentTypeDto ContentTypeAsDto(IContentType t, int count = -1)
	    {
	        Log.A($"for json a:{t.AppId}, type:{t.Name}");
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
                Description = t.Description.UseFallbackIfNoValue(details?.Description),
                EditInfo = new EditInfoDto(t),
                UsesSharedDef = ancestorDecorator != null,
                SharedDefId = ancestorDecorator?.Id,
                Items = count,
                Fields = t.Attributes.Count,
                Metadata = (ser as ConvertToEavLight)?.CreateListOfSubEntities(t.Metadata,
                    SubEntitySerialization.AllTrue()),
                Properties = properties,
                Permissions = new HasPermissionsDto { Count = t.Metadata.Permissions.Count() },
            };
	        return jsonReady;
	    }

	    public ContentTypeDto GetSingle(string contentTypeStaticName, string scope = null)
	    {
	        var wrapLog = Log.Fn<ContentTypeDto>($"a#{_appId}, type:{contentTypeStaticName}, scope:{scope}");
            var appState = _appStates.Get(_appId);

            var ct = appState.GetContentType(contentTypeStaticName);
            return wrapLog.Return(ContentTypeAsDto(ct));
	    }

	    public bool Delete(string staticName)
	    {
	        Log.A($"delete a#{_appId}, name:{staticName}");
            GetDb().ContentType.Delete(staticName);
	        return true;
	    }

	    public bool Save(Dictionary<string, string> item)
	    {
	        Log.A($"save a#{_appId}, item count:{item?.Count}");
	        if (item == null)
	        {
	            Log.A("item was null, will cancel");
	            return false;
	        }

	        GetDb().ContentType.AddOrUpdate(
                item["StaticName"], 
                item["Scope"], 
                item["Name"], 
                item["Description"],
                null, false);
	        return true;
	    }
        #endregion

        public bool CreateGhost(string sourceStaticName)
	    {
	        Log.A($"create ghost a#{_appId}, type:{sourceStaticName}");
	        GetDb().ContentType.CreateGhost(sourceStaticName);
            return true;
	    }

        #region Fields - Get, Reorder, Data-Types (for dropdown), etc.

        /// <summary>
        /// Returns the configuration for a content type
        /// </summary>
        public IEnumerable<ContentTypeFieldDto> GetFields(string staticName)
        {
            Log.A($"get fields a#{_appId}, type:{staticName}");
            var appState = _appStates.Get(_appId);

            if (!(appState.GetContentType(staticName) is IContentType type))
            {
                //throw new Exception("type should be a ContentType - something broke");
                Log.A($"error, type:{staticName} is null, it is missing or it is not a ContentType - something broke");
                return new List<ContentTypeFieldDto>();
            }

            var fields = type.Attributes.OrderBy(a => a.SortOrder);

            var hasAncestor = type.HasAncestor();
            var ancestorDecorator = type.GetDecorator<IAncestor>();

            var appInputTypes = _appRuntimeLazy.Value.Init(Log).Init(_appId, true).ContentTypes.GetInputTypes();

            var ser = _convertToEavLight.Value;
            return fields.Select(a =>
            {
                var inputType = FindInputType(a);
                return new ContentTypeFieldDto
                {
                    Id = a.AttributeId,
                    SortOrder = a.SortOrder,
                    Type = a.Type,
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
                    InputTypeConfig = appInputTypes.FirstOrDefault(it => it.Type == inputType),
                    Permissions = new HasPermissionsDto { Count = a.Metadata.Permissions.Count() },

                    // new in 12.01
                    IsEphemeral = a.Metadata.GetBestValue<bool>(AttributeMetadata.MetadataFieldAllIsEphemeral,
                        AttributeMetadata.TypeGeneral),
                    HasFormulas = HasCalculations(a),

                    // Read-Only new in v13
                    EditInfo = new EditInfoDto(type),
                };
            });
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
            return string.IsNullOrEmpty(inputType) ? "unknown" : inputType;
        }

        private bool HasCalculations(IContentTypeAttribute attribute)
        {
            var wrapLog = Log.Fn<bool>(attribute.Name);
            var allMd = attribute.Metadata.FirstOrDefaultOfType(AttributeMetadata.TypeGeneral);
            if (allMd == null) return wrapLog.ReturnFalse("no @All");

            var calculationsAttr = allMd.Attributes.Values.FirstOrDefault(a => a.Name == AttributeMetadata.MetadataFieldAllFormulas);
            if (calculationsAttr == null) return wrapLog.ReturnFalse("no calc property");

            var calculations = calculationsAttr.Values?.FirstOrDefault()?.ObjectContents as IEnumerable<IEntity>;
            return wrapLog.Return(calculations?.Any() ?? false);
        }


        public bool Reorder(int contentTypeId, string newSortOrder)
        {
            Log.A($"reorder type#{contentTypeId}, order:{newSortOrder}");

            var sortOrderList = newSortOrder.Trim('[', ']').Split(',').Select(int.Parse).ToList();
            GetDb().ContentType.SortAttributes(contentTypeId, sortOrderList);
            return true;
        }

	    public string[] DataTypes()
	    {
	        Log.A($"get data types");
            return GetDb().Attributes.DataTypeNames();
	    }


        public int AddField(int contentTypeId, string staticName, string type, string inputType, int sortOrder)
	    {
	        Log.A($"add field type#{contentTypeId}, name:{staticName}, type:{type}, input:{inputType}, order:{sortOrder}");
            var attDef = new ContentTypeAttribute(AppManager.AppId, staticName, type, false, 0, sortOrder);
            return AppManager.ContentTypes.CreateAttributeAndInitializeAndSave(contentTypeId, attDef, inputType);
	    }

        public bool SetInputType(int attributeId, string inputType)
        {
            Log.A($"update input type attrib:{attributeId}, input:{inputType}");
            return AppManager.ContentTypes.UpdateInputType(attributeId, inputType);
        }

	    public bool DeleteField(int contentTypeId, int attributeId)
	    {
	        Log.A($"delete field type#{contentTypeId}, attrib:{attributeId}");
            return GetDb().Attributes.RemoveAttributeAndAllValuesAndSave(attributeId);
	    }

	    public void SetTitle(int contentTypeId, int attributeId)
	    {
	        Log.A($"set title type#{contentTypeId}, attrib:{attributeId}");
	        GetDb().Attributes.SetTitleAttribute(attributeId, contentTypeId);
	    }

        public bool Rename(int contentTypeId, int attributeId, string newName)
        {
            Log.A($"rename attribute type#{contentTypeId}, attrib:{attributeId}, name:{newName}");
            GetDb().Attributes.RenameAttribute(attributeId, contentTypeId, newName);
            return true;
        }

        #endregion

        private DbDataController GetDb() => _db ?? (_db = _dbLazy.Value.Init(Log).Init(null, _appId));
        private DbDataController _db;
    }


}