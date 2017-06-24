using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Data;

namespace ToSic.Eav.Apps.Parts
{
    public class ContentTypeManager : BaseManager
    {
        public ContentTypeManager(AppManager app) : base(app)
        {
        }

        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// Simple overload returning int so it can be used from outside
        /// </summary>
        public int CreateAttributeAndInitializeAndSave(int attributeSetId, string staticName, string type, string inputType, int sortOrder)//, int attributeGroupId, bool isTitle)
        {
            // todo: not nice deep-access, improve some time
            var newAttribute = _appManager.DataController.AttributesDefinition.AddAttributeAndSave(null, attributeSetId, staticName, type, sortOrder, 1, false);

            // set the nice name and input type, important for newly created attributes
            InitializeNameAndInputType(staticName, inputType, newAttribute);

            return newAttribute;
        }

        private void InitializeNameAndInputType(string staticName, string inputType, int attributeId)
        {
            // new: set the inputType - this is a bit tricky because it needs an attached entity of type "@All" to set the value to...
            var newValues = new Dictionary<string, object>
            {
                {"VisibleInEditUI", true},
                {"Name", staticName},
                {"InputType", inputType}
            };
            var meta = new Metadata
            {
                HasMetadata = true,
                TargetType = Constants.MetadataForField,
                KeyNumber = attributeId
            };
            _appManager.Entities.TempAddMetadata(meta, "@All", newValues); // todo: put "@All" into some constant
        }

        public bool UpdateInputType(int attributeId, string inputType)
        {
            var newValues = new Dictionary<string, object> { { "InputType", inputType } };

            var meta = new Metadata
            {
                HasMetadata = true,
                TargetType = Constants.MetadataForField,
                KeyNumber = attributeId
            };
            _appManager.Entities.TempAddMetadata(meta, "@All", newValues); // todo: put "@All" into some constant
            return true;
        }

    }
}
