using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Conversion;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Repository.Efc;
using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi
{
	/// <inheritdoc />
	/// <summary>
	/// Web API Controller for ContentTypes
	/// </summary>
	public class ContentTypeController : HasLog
    {
        public ContentTypeController(ILog parentLog = null) : base("Api.EavCTC", parentLog, "get EavCTC")
        {
        }

        #region Content-Type Get, Delete, Save
	    public IEnumerable<ContentTypeInfo> Get(int appId, string scope = null, bool withStatistics = false)
        {
            var wrapLog = Log.Call($"a#{appId}, scope:{scope}, stats:{withStatistics}");

            // should use app-manager and return each type 1x only
            var appMan = new AppManager(appId, Log);

            // 2020-01-15 2sxc 10.27.00 Special side-effect, pre-generate the resources, settings etc. if they didn't exist yet
            if (scope == AppConstants.ScopeApp)
            {
                Log.Add($"is scope {scope}, will do extra processing");
                appMan.EnsureAppIsConfigured(); // make sure additional settings etc. exist
            }

            // get all types
            var allTypes = appMan.Read.ContentTypes.FromScope(scope, true);

            var filteredType = allTypes.Where(t => t.Scope == scope)
                .OrderBy(t => t.Name)
                .Select(t => ContentTypeForJson(t as ContentType, appMan.Read.Entities.Get(t.Name).Count()));
            wrapLog("ok");
            return filteredType;
	    }

        private ContentTypeInfo ContentTypeForJson(ContentType t, int count = -1)
	    {
	        Log.Add($"for json a:{t.AppId}, type:{t.Name}");
	        var metadata = t.Metadata.Description;

	        var nameOverride = metadata?.GetBestValue(Constants.ContentTypeMetadataLabel).ToString();
	        if (string.IsNullOrEmpty(nameOverride))
	            nameOverride = t.Name;
            var ser = new EntitiesToDictionary();

	        var share = (IContentTypeShared) t;

	        var jsonReady = new ContentTypeInfo
	        {
	            Id = t.ContentTypeId,
	            Name = t.Name,
	            Label = nameOverride,
	            StaticName = t.StaticName,
	            Scope = t.Scope,
	            Description = t.Description,
	            UsesSharedDef = share.ParentId != null,
	            SharedDefId = share.ParentId,
	            Items = count,
	            Fields = t.Attributes.Count,
	            Metadata = ser.Convert(metadata),
	        };
	        return jsonReady;
	    }

        [HttpGet]
	    public ContentTypeInfo GetSingle(int appId, string contentTypeStaticName, string scope = null)
	    {
	        var wrapLog = Log.Call($"a#{appId}, type:{contentTypeStaticName}, scope:{scope}");
            var appState = State.Get(appId);

            var ct = appState.GetContentType(contentTypeStaticName);
            wrapLog(null);
            return ContentTypeForJson(ct as ContentType);
	    }

        [HttpGet]
	    [HttpDelete]
	    public bool Delete(int appId, string staticName)
	    {
	        Log.Add($"delete a#{appId}, name:{staticName}");
            GetDb(appId).ContentType.Delete(staticName);
	        return true;
	    }

	    public bool Save(int appId, Dictionary<string, string> item)
	    {
	        Log.Add($"save a#{appId}, item count:{item?.Count}");
	        if (item == null)
	        {
	            Log.Add("item was null, will cancel");
	            return false;
	        }

	        GetDb(appId).ContentType.AddOrUpdate(
                item["StaticName"], 
                item["Scope"], 
                item["Name"], 
                item["Description"],
                null, false);
	        return true;
	    }
        #endregion

	    public bool CreateGhost(int appId, string sourceStaticName)
	    {
	        Log.Add($"create ghost a#{appId}, type:{sourceStaticName}");
	        GetDb(appId).ContentType.CreateGhost(sourceStaticName);
            return true;
	    }

        #region Fields - Get, Reorder, Data-Types (for dropdown), etc.

        /// <summary>
        /// Returns the configuration for a content type
        /// </summary>
        public IEnumerable<ContentTypeFieldInfo> GetFields(int appId, string staticName)
        {
            Log.Add($"get fields a#{appId}, type:{staticName}");
            var appState = State.Get(appId);
            if (!(appState.GetContentType(staticName) is ContentType type))
                throw new Exception("type should be a ContentType - something broke");
            var fields = type.Attributes.OrderBy(a => a.SortOrder);


            var appInputTypes = new AppRuntime(appId, true, Log).ContentTypes.GetInputTypes();

            var ser = new EntitiesToDictionary();
            return fields.Select(a =>
            {
                var inputType = FindInputType(a);
                return new ContentTypeFieldInfo
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
            var inputType = attribute.Metadata.GetBestValue<string>(Constants.MetadataFieldAllInputType, Constants.MetadataFieldTypeAll);

            // unknown will let the UI fallback on other mechanisms
            return string.IsNullOrEmpty(inputType) ? "unknown" : inputType;
        }



        public bool Reorder(int appId, int contentTypeId, string newSortOrder)
        {
            Log.Add($"reorder a#{appId}, type#{contentTypeId}, order:{newSortOrder}");

            var sortOrderList = newSortOrder.Trim('[', ']').Split(',').Select(int.Parse).ToList();
            GetDb(appId).ContentType.SortAttributes(contentTypeId, sortOrderList);
            return true;
        }

	    public string[] DataTypes(int appId)
	    {
	        Log.Add($"get data types a#{appId}");
            return GetDb(appId).Attributes.DataTypeNames();
	    }

	    public List<InputTypeInfo> InputTypes(int appId)
	    {
	        Log.Add($"get input types a#{appId}");
	        var appInputTypes = new AppRuntime(appId, true, Log).ContentTypes.GetInputTypes();

	        return appInputTypes;
	    }

           
            
        public int AddField(int appId, int contentTypeId, string staticName, string type, string inputType, int sortOrder)
	    {
	        Log.Add($"add field a#{appId}, type#{contentTypeId}, name:{staticName}, type:{type}, input:{inputType}, order:{sortOrder}");
            var attDef = new ContentTypeAttribute(appId, staticName, type, false, 0, sortOrder);
	        var appManager = new AppManager(appId, Log);

            return appManager.ContentTypes.CreateAttributeAndInitializeAndSave(contentTypeId, attDef, inputType);
	    }

        public bool UpdateInputType(int appId, int attributeId, string inputType)
        {
            Log.Add($"update input type a#{appId}, attrib:{attributeId}, input:{inputType}");
            var appManager = new AppManager(appId, Log);
            return appManager.ContentTypes.UpdateInputType(attributeId, inputType);
        }

	    public bool DeleteField(int appId, int contentTypeId, int attributeId)
	    {
	        Log.Add($"delete field a#{appId}, type#{contentTypeId}, attrib:{attributeId}");
            return GetDb(appId).Attributes.RemoveAttributeAndAllValuesAndSave(attributeId);
	    }

	    public void SetTitle(int appId, int contentTypeId, int attributeId)
	    {
	        Log.Add($"set title a#{appId}, type#{contentTypeId}, attrib:{attributeId}");
	        GetDb(appId).Attributes.SetTitleAttribute(attributeId, contentTypeId);
	    }

        public bool Rename(int appId, int contentTypeId, int attributeId, string newName)
        {
            Log.Add($"rename attribute a#{appId}, type#{contentTypeId}, attrib:{attributeId}, name:{newName}");
            GetDb(appId).Attributes.RenameAttribute(attributeId, contentTypeId, newName);
            return true;
        }

        #endregion

        private DbDataController GetDb(int appId) => DbDataController.Instance(null, appId, Log);
    }


}