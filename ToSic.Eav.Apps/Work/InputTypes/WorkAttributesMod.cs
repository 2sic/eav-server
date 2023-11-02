using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps.Work
{
    public class WorkAttributesMod : WorkUnitBase<IAppWorkCtxWithDb>
    {
        private readonly AppWork _appWork;

        public WorkAttributesMod(AppWork appWork) : base("ApS.InpGet")
        {
            ConnectServices(
                _appWork = appWork
            );
        }

        public void Create(string nameId, string scope) =>
            AppWorkCtx.DataController.DoAndSave(() =>
                AppWorkCtx.DataController.AttribSet.PrepareDbAttribSet(nameId, nameId, scope, false, AppWorkCtx.AppId));

        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// Simple overload returning int so it can be used from outside
        /// </summary>
        public int CreateAttributeAndInitializeAndSave(int attributeSetId, ContentTypeAttribute attDef, string inputType)
        {
            var l = Log.Fn<int>($"type:{attributeSetId}, input:{inputType}");
            var newAttribute = AppWorkCtx.DataController.Attributes.AddAttributeAndSave(attributeSetId, attDef);

            // set the nice name and input type, important for newly created attributes
            InitializeNameAndInputType(attDef.Name, inputType, newAttribute);

            return l.ReturnAndLog(newAttribute);
        }

        public bool UpdateInputType(int attributeId, string inputType)
        {
            var l = Log.Fn<bool>($"attrib:{attributeId}, input:{inputType}");
            var newValues = new Dictionary<string, object> { { AttributeMetadata.GeneralFieldInputType, inputType } };

            var meta = new Target((int)TargetTypes.Attribute, null, keyNumber: attributeId);
            // #ExtractEntitySave - verified
            _appWork.EntityMetadata(AppWorkCtx).SaveMetadata(meta, AttributeMetadata.TypeGeneral, newValues);
            return l.ReturnTrue();
        }

        private void InitializeNameAndInputType(string staticName, string inputType, int attributeId)
        {
            var l = Log.Fn($"attrib:{attributeId}, name:{staticName}, input:{inputType}");
            // new: set the inputType - this is a bit tricky because it needs an attached entity of type @All to set the value to...
            var newValues = new Dictionary<string, object>
            {
                { "VisibleInEditUI", true },
                { "Name", staticName },
                { AttributeMetadata.GeneralFieldInputType, inputType }
            };
            var meta = new Target((int)TargetTypes.Attribute, null, keyNumber: attributeId);
            // #ExtractEntitySave - verified
            _appWork.EntityMetadata(AppWorkCtx).SaveMetadata(meta, AttributeMetadata.TypeGeneral, newValues);
            l.Done();
        }

    }
}
