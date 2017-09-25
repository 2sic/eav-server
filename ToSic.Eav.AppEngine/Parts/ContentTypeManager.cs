using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.Parts
{
    public class ContentTypeManager : ManagerBase
    {
        public ContentTypeManager(AppManager app, Log parentLog) : base(app, parentLog, "CTMngr")
        {
        }

        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// Simple overload returning int so it can be used from outside
        /// </summary>
        public int CreateAttributeAndInitializeAndSave(int attributeSetId, AttributeDefinition attDef, /*string staticName, string type,*/ string inputType/*, int sortOrder*/)
        {
            Log.Add($"create attrib+init+save type:{attributeSetId}, input:{inputType}");
            var newAttribute = _appManager.DataController.AttributesDefinition.AddAttributeAndSave(attributeSetId, attDef);// staticName, type, sortOrder, false);

            // set the nice name and input type, important for newly created attributes
            InitializeNameAndInputType(/*staticName*/attDef.Name, inputType, newAttribute);

            return newAttribute;
        }

        private void InitializeNameAndInputType(string staticName, string inputType, int attributeId)
        {
            Log.Add($"init name+input attrib:{attributeId}, name:{staticName}, input:{inputType}");
            // new: set the inputType - this is a bit tricky because it needs an attached entity of type "@All" to set the value to...
            var newValues = new Dictionary<string, object>
            {
                {"VisibleInEditUI", true},
                {"Name", staticName},
                {"InputType", inputType}
            };
            var meta = new Metadata
            {
                TargetType = Constants.MetadataForAttribute,
                KeyNumber = attributeId
            };
            _appManager.Entities.SaveMetadata(meta, "@All", newValues); // todo: put "@All" into some constant
        }

        public bool UpdateInputType(int attributeId, string inputType)
        {
            Log.Add($"update input type attrib:{attributeId}, input:{inputType}");
            var newValues = new Dictionary<string, object> { { "InputType", inputType } };

            var meta = new Metadata
            {
                TargetType = Constants.MetadataForAttribute,
                KeyNumber = attributeId
            };
            _appManager.Entities.SaveMetadata(meta, "@All", newValues); // todo: put "@All" into some constant
            return true;
        }

        

    }
}
