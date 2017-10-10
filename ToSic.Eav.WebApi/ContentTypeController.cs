﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Caches;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Serializers;

namespace ToSic.Eav.WebApi
{
	/// <inheritdoc />
	/// <summary>
	/// Web API Controller for ContentTypes
	/// </summary>
	public class ContentTypeController : Eav3WebApiBase
    {
        public ContentTypeController(Log parentLog = null) : base(parentLog)
        {
            Log.Rename("EavCTC");
        }

        #region Content-Type Get, Delete, Save
        [HttpGet]
	    public IEnumerable<dynamic> Get(int appId, string scope = null, bool withStatistics = false)
        {
            Log.Add($"get a#{appId}, scope:{scope}, stats:{withStatistics}");
            // scope can be null (eav) or alternatives would be "System", "2SexyContent-System", "2SexyContent-App", "2SexyContent"
            var cache = (BaseCache)DataSource.GetCache(null, appId);
            var allTypes = cache.GetContentTypes();

            var filteredType = allTypes.Where(t => t.Scope == scope)
                .OrderBy(t => t.Name)
                .Select(t => ContentTypeForJson(t, cache));

            return filteredType;
	    }

	    private dynamic ContentTypeForJson(IContentType t, ICache cache)
	    {
	        Log.Add($"for json a:{t.AppId}, type:{t.Name}");
	        var metadata = t.Items.FirstOrDefault();

	        var nameOverride = metadata?.GetBestValue(Constants.ContentTypeMetadataLabel).ToString();
	        if (string.IsNullOrEmpty(nameOverride))
	            nameOverride = t.Name;
            var ser = new Serializer();

	        var share = (IUsesSharedDefinition) t;

            var jsonReady = new
	        {
	            Id = t.ContentTypeId,
                t.Name,
                Label = nameOverride,
	            t.StaticName,
	            t.Scope,
	            t.Description,
	            UsesSharedDef = share.ParentId != null,
	            SharedDefId = share.ParentId,
	            Items = cache.LightList.Count(i => i.Type == t),
	            Fields = ((ContentType)t).Attributes.Count,
                Metadata = ser.Prepare(metadata)
	        };
	        return jsonReady;
	    }

        [HttpGet]
	    public dynamic GetSingle(int appId, string contentTypeStaticName, string scope = null)
	    {
	        Log.Add($"get single a#{appId}, type:{contentTypeStaticName}, scope:{scope}");
            SetAppIdAndUser(appId);
            var cache = DataSource.GetCache(null, appId);
            var ct = cache.GetContentType(contentTypeStaticName);
            return ContentTypeForJson(ct, cache);
	    }

        [HttpGet]
	    [HttpDelete]
	    public bool Delete(int appId, string staticName)
	    {
	        Log.Add($"delete a#{appId}, name:{staticName}");
            SetAppIdAndUser(appId);
            CurrentContext.ContentType.Delete(staticName);
	        return true;
	    }

	    [HttpPost]
	    public bool Save(int appId, Dictionary<string, string> item)
	    {
	        Log.Add($"save a#{appId}, item count:{item.Count}");
            SetAppIdAndUser(appId);
	        bool.TryParse(item["ChangeStaticName"], out var changeStaticName);
            CurrentContext.ContentType.AddOrUpdate(
                item["StaticName"], 
                item["Scope"], 
                item["Name"], 
                item["Description"],
                null, false, 
                changeStaticName, 
                changeStaticName ? item["NewStaticName"] : item["StaticName"]);
	        return true;
	    }
        #endregion

        [HttpGet]
	    public bool CreateGhost(int appId, string sourceStaticName)
	    {
	        Log.Add($"create ghost a#{appId}, type:{sourceStaticName}");
            SetAppIdAndUser(appId);
            CurrentContext.ContentType.CreateGhost(sourceStaticName);
            return true;
	    }

        #region Fields - Get, Reorder, Data-Types (for dropdown), etc.
        /// <summary>
        /// Returns the configuration for a content type
        /// </summary>
        [HttpGet]
        public IEnumerable<dynamic> GetFields(int appId, string staticName)
        {
            Log.Add($"get fields a#{appId}, type:{staticName}");
            SetAppIdAndUser(appId);

            SetAppIdAndUser(appId);
            var fields = DataSource.GetCache(null, appId)
                .GetContentType(staticName)
                .Attributes
                .OrderBy(a => a.SortOrder);


            var appInputTypes = new AppRuntime(appId).ContentTypes.GetInputTypes(true).ToList();
            var noTitleCount = 0;
            var fldName = "";

            // assemble a list of all input-types (like "string-default", "string-wysiwyg..."
            Dictionary<string, IEntity> inputTypesDic;
            try
            {
                inputTypesDic =
                    appInputTypes.ToDictionary(
                        a => fldName = a.GetBestTitle() ?? "error-no-title" + noTitleCount++,
                        a => a);
            }
            catch (Exception ex)
            {
                throw new Exception("Error on " + fldName + "; note: noTitleCount " + noTitleCount, ex);
            }

            var ser = new Serializer();
            return fields.Select(a =>
            {
                var inputtype = FindInputType(a.Items);
                return new
                {
                    Id = a.AttributeId,
                    a.SortOrder,
                    a.Type,
                    InputType = inputtype,
                    StaticName = a.Name,
                    a.IsTitle,
                    a.AttributeId,
                    Metadata = a.Items.ToDictionary(e => e.Type.StaticName.TrimStart('@'), e => ser.Prepare(e)),
                    InputTypeConfig = inputTypesDic.ContainsKey(inputtype) 
                        ? ser.Prepare(inputTypesDic[inputtype]) : null
                };
            });
        }

	    private static string FindInputType(List<IEntity> definitions)
	    {
	        var inputType = definitions.FirstOrDefault(d => d.Type.StaticName == "@All")
                ?.GetBestValue("InputType");

	        return string.IsNullOrEmpty(inputType as string) ? "unknown" : inputType.ToString();
	    }

        [HttpGet]
        public bool Reorder(int appId, int contentTypeId, string newSortOrder)
        {
            Log.Add($"reorder a#{appId}, type#{contentTypeId}, order:{newSortOrder}");
            SetAppIdAndUser(appId);

            var sortOrderList = newSortOrder.Trim('[', ']').Split(',').Select(int.Parse).ToList();
            CurrentContext.ContentType.SortAttributes(contentTypeId, sortOrderList);
            return true;
        }

	    [HttpGet]
	    public string[] DataTypes(int appId)
	    {
	        Log.Add($"get data types a#{appId}");
            SetAppIdAndUser(appId);
            return CurrentContext.AttributesDefinition.DataTypeNames(appId);
	    }

	    [HttpGet]
	    // public IEnumerable<Dictionary<string, object>> InputTypes(int appId)
	    public IEnumerable<Dictionary<string, object>> InputTypes(int appId)
	    {
	        Log.Add($"get input types a#{appId}");
            SetAppIdAndUser(appId);
	        var appInputTypes = new AppRuntime(appId).ContentTypes.GetInputTypes(true).ToList(); // appDef.GetInputTypes(true).ToList();
            
	        return Serializer.Prepare(appInputTypes);

	    }

           
            
        [HttpGet]
        [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
        public int AddField(int appId, int contentTypeId, string staticName, string type, string inputType, int sortOrder)
	    {
	        Log.Add($"add field a#{appId}, type#{contentTypeId}, name:{staticName}, type:{type}, input:{inputType}, order:{sortOrder}");
            SetAppIdAndUser(appId);
            var attDef = new AttributeDefinition(appId, staticName, type, false, 0, sortOrder);
            
	        return AppManager.ContentTypes.CreateAttributeAndInitializeAndSave(contentTypeId, attDef, /*staticName, type, */inputType/*, sortOrder*/);
	    }

        [HttpGet]
        public bool UpdateInputType(int appId, int attributeId, string inputType)
        {
            Log.Add($"update input type a#{appId}, attrib:{attributeId}, input:{inputType}");
            SetAppIdAndUser(appId);
            return AppManager.ContentTypes.UpdateInputType(attributeId, inputType);
        }

        [HttpGet]
        [HttpDelete]
	    public bool DeleteField(int appId, int contentTypeId, int attributeId)
	    {
	        Log.Add($"delete field a#{appId}, type#{contentTypeId}, attrib:{attributeId}");
            SetAppIdAndUser(appId);
            return CurrentContext.AttributesDefinition.RemoveAttributeAndAllValuesAndSave(attributeId);
	    }

        [HttpGet]
	    public void SetTitle(int appId, int contentTypeId, int attributeId)
	    {
	        Log.Add($"set title a#{appId}, type#{contentTypeId}, attrib:{attributeId}");
            SetAppIdAndUser(appId);
            CurrentContext.AttributesDefinition.SetTitleAttribute(attributeId, contentTypeId);
	    }

        [HttpGet]
        public bool Rename(int appId, int contentTypeId, int attributeId, string newName)
        {
            Log.Add($"rename attribute a#{appId}, type#{contentTypeId}, attrib:{attributeId}, name:{newName}");
            SetAppIdAndUser(appId);
            CurrentContext.AttributesDefinition.RenameAttribute(attributeId, contentTypeId, newName);
            return true;
        }


        #endregion

    }
}