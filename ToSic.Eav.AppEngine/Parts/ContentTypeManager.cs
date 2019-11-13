using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    public class ContentTypeManager : ManagerBase
    {
        public ContentTypeManager(AppManager app, ILog parentLog) : base(app, parentLog, "App.TypMng")
        {
        }

        public void Create(string name, string staticName, string description, string scope)
        {
            AppManager.DataController.DoAndSave(() =>
                AppManager.DataController.AttribSet.PrepareDbAttribSet(name, description, name, scope, false, AppManager.AppId));

            // ensure app-package loads this type - this is a simple thing, as no data uses this type till now
            AppManager.Package.ContentTypesShouldBeReloaded = true;
        }

        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// Simple overload returning int so it can be used from outside
        /// </summary>
        public int CreateAttributeAndInitializeAndSave(int attributeSetId, ContentTypeAttribute attDef, string inputType)
        {
            Log.Add($"create attrib+init+save type:{attributeSetId}, input:{inputType}");
            var newAttribute = AppManager.DataController.AttributesDefinition.AddAttributeAndSave(attributeSetId, attDef);

            // set the nice name and input type, important for newly created attributes
            InitializeNameAndInputType(attDef.Name, inputType, newAttribute);

            return newAttribute;
        }

        private void InitializeNameAndInputType(string staticName, string inputType, int attributeId)
        {
            Log.Add($"init name+input attrib:{attributeId}, name:{staticName}, input:{inputType}");
            // new: set the inputType - this is a bit tricky because it needs an attached entity of type @All to set the value to...
            var newValues = new Dictionary<string, object>
            {
                {"VisibleInEditUI", true},
                {"Name", staticName},
                {Constants.MetadataFieldAllInputType, inputType}
            };
            var meta = new MetadataFor
            {
                TargetType = Constants.MetadataForAttribute,
                KeyNumber = attributeId
            };
            AppManager.Entities.SaveMetadata(meta, Constants.MetadataFieldTypeAll, newValues);
        }

        public bool UpdateInputType(int attributeId, string inputType)
        {
            Log.Add($"update input type attrib:{attributeId}, input:{inputType}");
            var newValues = new Dictionary<string, object> { { Constants.MetadataFieldAllInputType, inputType } };

            var meta = new MetadataFor
            {
                TargetType = Constants.MetadataForAttribute,
                KeyNumber = attributeId
            };
            AppManager.Entities.SaveMetadata(meta, Constants.MetadataFieldTypeAll, newValues);
            return true;
        }

        

    }
}
