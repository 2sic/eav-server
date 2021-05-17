using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Conversion;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.Security;
using static ToSic.Eav.Constants;

namespace ToSic.Eav.WebApi
{
	/// <inheritdoc />
	/// <summary>
	/// Web API Controller for ContentTypes
	/// </summary>
	public class ContentTypeApi : HasLog<ContentTypeApi>
    {

        #region Constructor / DI

        private readonly Lazy<AppRuntime> _appRuntimeLazy;
        private readonly Lazy<AppManager> _appManagerLazy;
        private readonly Lazy<DbDataController> _dbLazy;
        private readonly AppInitializedChecker _appInitializedChecker;

        private AppManager AppManager { get; set; }

        public ContentTypeApi(Lazy<AppRuntime> appRuntimeLazy, Lazy<AppManager> appManagerLazy, Lazy<DbDataController> dbLazy, AppInitializedChecker appInitializedChecker) : base("Api.EavCTC")
        {
            _appRuntimeLazy = appRuntimeLazy;
            _appManagerLazy = appManagerLazy;
            _dbLazy = dbLazy;
            _appInitializedChecker = appInitializedChecker;
        }

        public ContentTypeApi Init(int appId, ILog parentLog)
        {
            Log.LinkTo(parentLog);
            Log.Add($"Will use app {appId}");
            _appId = appId;
            AppManager = _appManagerLazy.Value.Init(appId, Log);
            return this;
        }

        private int _appId;

        #endregion

        #region Content-Type Get, Delete, Save

        // todo: rename to "List" to match external name, once feature/oqtane2 branch isn't used any more
        public IEnumerable<ContentTypeDto> Get(string scope = null, bool withStatistics = false)
        {
            var wrapLog = Log.Call($"scope:{scope}, stats:{withStatistics}");


            // 2020-01-15 2sxc 10.27.00 Special side-effect, pre-generate the resources, settings etc. if they didn't exist yet
            // this is important on "Content" apps, because these don't auto-initialize when loading from the DB
            // so for these, we must pre-ensure that the app is initialized as needed, if they 
            // are editing the resources etc. 
            if (scope == AppConstants.ScopeApp)
            {
                Log.Add($"is scope {scope}, will do extra processing");
                var appState = State.Get(AppManager);
                // make sure additional settings etc. exist
                _appInitializedChecker.EnsureAppConfiguredAndInformIfRefreshNeeded(appState, null, Log); 
            }
            // should use app-manager and return each type 1x only
            var appMan = AppManager;

            // get all types
            var allTypes = appMan.Read.ContentTypes.All.OfScope(scope, true);

            var filteredType = allTypes.Where(t => t.Scope == scope)
                .OrderBy(t => t.Name)
                .Select(t => ContentTypeForJson(t as ContentType, appMan.Read.Entities.Get(t.Name).Count()));
            wrapLog("ok");
            return filteredType;
	    }

        private ContentTypeDto ContentTypeForJson(ContentType t, int count = -1)
	    {
	        Log.Add($"for json a:{t.AppId}, type:{t.Name}");
	        var metadata = t.Metadata.Description;

	        var nameOverride = metadata?.Value<string>(ContentTypeMetadataLabel);
	        if (string.IsNullOrEmpty(nameOverride))
	            nameOverride = t.Name;
            var ser = new EntitiesToDictionary();

	        var shareInfo = (IContentTypeShared) t;

	        var jsonReady = new ContentTypeDto
	        {
	            Id = t.ContentTypeId,
	            Name = t.Name,
	            Label = nameOverride,
	            StaticName = t.StaticName,
	            Scope = t.Scope,
	            Description = t.Description,
	            UsesSharedDef = shareInfo.ParentId != null,
	            SharedDefId = shareInfo.ParentId,
	            Items = count,
	            Fields = t.Attributes.Count,
	            Metadata = ser.Convert(metadata),
                Permissions = new HasPermissionsDto { Count = t.Metadata.Permissions.Count()},
	        };
	        return jsonReady;
	    }

	    public ContentTypeDto GetSingle(string contentTypeStaticName, string scope = null)
	    {
	        var wrapLog = Log.Call($"a#{_appId}, type:{contentTypeStaticName}, scope:{scope}");
            var appState = State.Get(_appId);

            var ct = appState.GetContentType(contentTypeStaticName);
            wrapLog(null);
            return ContentTypeForJson(ct as ContentType);
	    }

	    public bool Delete(string staticName)
	    {
	        Log.Add($"delete a#{_appId}, name:{staticName}");
            GetDb().ContentType.Delete(staticName);
	        return true;
	    }

	    public bool Save(Dictionary<string, string> item)
	    {
	        Log.Add($"save a#{_appId}, item count:{item?.Count}");
	        if (item == null)
	        {
	            Log.Add("item was null, will cancel");
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
	        Log.Add($"create ghost a#{_appId}, type:{sourceStaticName}");
	        GetDb().ContentType.CreateGhost(sourceStaticName);
            return true;
	    }

        #region Fields - Get, Reorder, Data-Types (for dropdown), etc.

        /// <summary>
        /// Returns the configuration for a content type
        /// </summary>
        public IEnumerable<ContentTypeFieldDto> GetFields(string staticName)
        {
            Log.Add($"get fields a#{_appId}, type:{staticName}");
            var appState = State.Get(_appId);
            if (!(appState.GetContentType(staticName) is ContentType type))
                throw new Exception("type should be a ContentType - something broke");
            var fields = type.Attributes.OrderBy(a => a.SortOrder);


            var appInputTypes = _appRuntimeLazy.Value.Init(State.Identity(null, _appId), true, Log).ContentTypes.GetInputTypes();

            var ser = new EntitiesToDictionary();
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
                                var name = Guid.TryParse(e.Type.StaticName, out _)
                                    ? e.Type.Name
                                    : e.Type.StaticName;
                                return name.TrimStart('@');
                            },
                            e => ser.Convert(e)
                        ),
                    InputTypeConfig = appInputTypes.FirstOrDefault(it => it.Type == inputType),
                    Permissions = new HasPermissionsDto {Count = a.Metadata.Permissions.Count()},

                    // new in 12.01
                    IsEphemeral = a.Metadata.GetBestValue<bool>(MetadataFieldAllIsEphemeral, MetadataFieldTypeAll),
                    HasCalculations = HasCalculations(a),
                };
            });
            
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
            var inputType = attribute.Metadata.GetBestValue<string>(MetadataFieldAllInputType, MetadataFieldTypeAll);

            // unknown will let the UI fallback on other mechanisms
            return string.IsNullOrEmpty(inputType) ? "unknown" : inputType;
        }

        private bool HasCalculations(IContentTypeAttribute attribute)
        {
            var wrapLog = Log.Call<bool>(attribute.Name);
            var allMd = attribute.Metadata.FirstOrDefaultOfType(MetadataFieldTypeAll);
            if (allMd == null) return wrapLog("no @All", false);

            var calculationsAttr = allMd.Attributes.Values.FirstOrDefault(a => a.Name == MetadataFieldAllCalculations);
            if (calculationsAttr == null) return wrapLog("no calc property", false);

            var calculations = calculationsAttr.Values?.FirstOrDefault()?.ObjectContents as IEnumerable<IEntity>;
            return wrapLog(null, calculations?.Any() ?? false);
        }


        public bool Reorder(int contentTypeId, string newSortOrder)
        {
            Log.Add($"reorder type#{contentTypeId}, order:{newSortOrder}");

            var sortOrderList = newSortOrder.Trim('[', ']').Split(',').Select(int.Parse).ToList();
            GetDb().ContentType.SortAttributes(contentTypeId, sortOrderList);
            return true;
        }

	    public string[] DataTypes()
	    {
	        Log.Add($"get data types");
            return GetDb().Attributes.DataTypeNames();
	    }


        public int AddField(int contentTypeId, string staticName, string type, string inputType, int sortOrder)
	    {
	        Log.Add($"add field type#{contentTypeId}, name:{staticName}, type:{type}, input:{inputType}, order:{sortOrder}");
            var attDef = new ContentTypeAttribute(AppManager.AppId, staticName, type, false, 0, sortOrder);
            return AppManager.ContentTypes.CreateAttributeAndInitializeAndSave(contentTypeId, attDef, inputType);
	    }

        public bool SetInputType(int attributeId, string inputType)
        {
            Log.Add($"update input type attrib:{attributeId}, input:{inputType}");
            return AppManager.ContentTypes.UpdateInputType(attributeId, inputType);
        }

	    public bool DeleteField(int contentTypeId, int attributeId)
	    {
	        Log.Add($"delete field type#{contentTypeId}, attrib:{attributeId}");
            return GetDb().Attributes.RemoveAttributeAndAllValuesAndSave(attributeId);
	    }

	    public void SetTitle(int contentTypeId, int attributeId)
	    {
	        Log.Add($"set title type#{contentTypeId}, attrib:{attributeId}");
	        GetDb().Attributes.SetTitleAttribute(attributeId, contentTypeId);
	    }

        public bool Rename(int contentTypeId, int attributeId, string newName)
        {
            Log.Add($"rename attribute type#{contentTypeId}, attrib:{attributeId}, name:{newName}");
            GetDb().Attributes.RenameAttribute(attributeId, contentTypeId, newName);
            return true;
        }

        #endregion

        private DbDataController GetDb() => _db ?? (_db = _dbLazy.Value.Init(null, _appId, Log));
        private DbDataController _db;
    }


}