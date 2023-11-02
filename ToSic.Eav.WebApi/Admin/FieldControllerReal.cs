using System;
using System.Collections.Generic;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Apps.Work;
using ToSic.Eav.Data;
using ToSic.Eav.WebApi.Dto;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.WebApi.Admin
{
    public class FieldControllerReal : ServiceBase, IFieldController
    {
        public const string LogSuffix = "Field";

        public FieldControllerReal(LazySvc<ContentTypeApi> ctApiLazy, AppWork appWork): base("Api.FieldRl")
        {
            ConnectServices(
                _ctApiLazy = ctApiLazy,
                _appWork = appWork
            );
        }

        private readonly AppWork _appWork;
        private readonly LazySvc<ContentTypeApi> _ctApiLazy;

        #region Fields - Get, Reorder, Data-Types (for dropdown), etc.

        public IEnumerable<ContentTypeFieldDto> All(int appId, string staticName) => _ctApiLazy.Value.Init(appId).GetFields(staticName);


        public string[] DataTypes(int appId) => _ctApiLazy.Value.Init(appId).DataTypes();


        public List<InputTypeInfo> InputTypes(int appId) => _appWork.InputTypesNew(_appWork.ContextPlus(appId)).GetInputTypes();


        public Dictionary<string, string> ReservedNames() => Attributes.ReservedNames;


        public int Add(int appId, int contentTypeId, string staticName, string type, string inputType, int index)
            => _ctApiLazy.Value.Init(appId).AddField(contentTypeId, staticName, type, inputType, index);


        public bool Delete(int appId, int contentTypeId, int attributeId)
            => _ctApiLazy.Value.Init(appId).DeleteField(contentTypeId, attributeId);


        public bool Sort(int appId, int contentTypeId, string order)
            => _ctApiLazy.Value.Init(appId).Reorder(contentTypeId, order);


        public bool InputType(int appId, int attributeId, string inputType)
            => _ctApiLazy.Value.Init(appId).SetInputType(attributeId, inputType);

        #endregion

        public void Rename(int appId, int contentTypeId, int attributeId, string newName)
            => _ctApiLazy.Value.Init(appId).Rename(contentTypeId, attributeId, newName);

        #region Shared Fields

        public IEnumerable<ContentTypeFieldDto> GetSharedFields(int appId) 
            => _ctApiLazy.Value.Init(appId).GetSharedFields();


        public void Share(int appId, int attributeId, bool share, bool hide = false)
            => _ctApiLazy.Value.Init(appId).FieldShare(attributeId, share, hide);

        public void Inherit(int appId, int attributeId, Guid inheritMetadataOf)
            => _ctApiLazy.Value.Init(appId).FieldInherit(attributeId, inheritMetadataOf);

        #endregion
    }
}