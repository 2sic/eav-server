using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.WebApi.Dto;

namespace ToSic.Eav.WebApi.Admin
{
    public class FieldControllerReal : HasLog<FieldControllerReal>, IFieldController
    {
        public const string LogSuffix = "Field";

        public FieldControllerReal(Lazy<AppRuntime> appRuntime, Lazy<ContentTypeApi> ctApiLazy): base("Api.FieldRl")
        {
            _appRuntime = appRuntime;
            _ctApiLazy = ctApiLazy;
        }
        private readonly Lazy<AppRuntime> _appRuntime;
        private readonly Lazy<ContentTypeApi> _ctApiLazy;

        #region Fields - Get, Reorder, Data-Types (for dropdown), etc.

        public IEnumerable<ContentTypeFieldDto> All(int appId, string staticName) => _ctApiLazy.Value.Init(appId, Log).GetFields(staticName);


        public string[] DataTypes(int appId) => _ctApiLazy.Value.Init(appId, Log).DataTypes();


        public List<InputTypeInfo> InputTypes(int appId) => _appRuntime.Value.Init(appId, true, Log).ContentTypes.GetInputTypes();


        public Dictionary<string, string> ReservedNames() => Attributes.ReservedNames;


        public int Add(int appId, int contentTypeId, string staticName, string type, string inputType, int index)
            => _ctApiLazy.Value.Init(appId, Log).AddField(contentTypeId, staticName, type, inputType, index);


        public bool Delete(int appId, int contentTypeId, int attributeId)
            => _ctApiLazy.Value.Init(appId, Log).DeleteField(contentTypeId, attributeId);


        public bool Sort(int appId, int contentTypeId, string order) => _ctApiLazy.Value.Init(appId, Log).Reorder(contentTypeId, order);


        public bool InputType(int appId, int attributeId, string inputType) => _ctApiLazy.Value.Init(appId, Log).SetInputType(attributeId, inputType);

        #endregion

        public void Rename(int appId, int contentTypeId, int attributeId, string newName) => _ctApiLazy.Value.Init(appId, Log).Rename(contentTypeId, attributeId, newName);
    }
}