using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Builder
{
    public class ContentTypeAttributeBuilder: ServiceBase
    {
        private readonly MultiBuilder _builder;
        private readonly AppState _globalApp;

        public ContentTypeAttributeBuilder(IAppStates appStates, MultiBuilder builder): base("Eav.CtAtBl")
        {
            ConnectServices(
                _builder = builder
            );
            _globalApp = appStates.GetPresetApp();
        }

        /// <summary>
        /// Shortcut to get an @All Entity Describing an Attribute
        /// </summary>
        public Entity GenerateAttributeMetadata(int appId, string name, string notes, bool? visibleInEditUi, string defaultValue, string inputType)
        {
            var valDic = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(name)) valDic.Add("Name", name);
            if (!string.IsNullOrEmpty(notes)) valDic.Add("Notes", notes);
            if (visibleInEditUi.HasValue) valDic.Add("VisibleInEditUI", visibleInEditUi);
            if (defaultValue != null) valDic.Add("DefaultValue", defaultValue);
            if (!string.IsNullOrEmpty(inputType)) valDic.Add(AttributeMetadata.GeneralFieldInputType, inputType);

            return _builder.Entity.Create(appId: appId, guid: Guid.Empty, contentType: _globalApp.GetContentType(AttributeMetadata.TypeGeneral), rawValues: valDic);
        }
    }
}
