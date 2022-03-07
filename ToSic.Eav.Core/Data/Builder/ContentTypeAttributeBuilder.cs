using System;
using System.Collections.Generic;
using ToSic.Eav.Apps;

namespace ToSic.Eav.Data.Builder
{
    public class ContentTypeAttributeBuilder
    {
        public ContentTypeAttributeBuilder(IAppStates appStates)
        {
            _globalApp = appStates.GetPresetApp();
        }
        private readonly AppState _globalApp;

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

            return new Entity(appId, Guid.Empty, _globalApp.GetContentType(AttributeMetadata.TypeGeneral), valDic);
        }
    }
}
