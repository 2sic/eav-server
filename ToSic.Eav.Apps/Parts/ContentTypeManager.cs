using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Apps.Work;

namespace ToSic.Eav.Apps.Parts
{
    public class ContentTypeManager : PartOf<AppManager>
    {
        private readonly AppWork _appWork;
        public ContentTypeManager(AppWork appWork) : base("App.TypMng")
        {
            ConnectServices(
                _appWork = appWork
            );
        }

        private IAppWorkCtxWithDb AppCtxWithDb => _appCtx ?? (_appCtx = _appWork.CtxWithDb(Parent.AppState, Parent.DataController));
        private IAppWorkCtxWithDb _appCtx;

        public void Create(string nameId, string scope)
        {
            Parent.DataController.DoAndSave(() =>
                Parent.DataController.AttribSet.PrepareDbAttribSet(nameId, nameId, scope, false, Parent.AppId));
        }

        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// Simple overload returning int so it can be used from outside
        /// </summary>
        public int CreateAttributeAndInitializeAndSave(int attributeSetId, ContentTypeAttribute attDef, string inputType)
        {
            var l = Log.Fn<int>($"type:{attributeSetId}, input:{inputType}");
            var newAttribute = Parent.DataController.Attributes.AddAttributeAndSave(attributeSetId, attDef);

            // set the nice name and input type, important for newly created attributes
            InitializeNameAndInputType(attDef.Name, inputType, newAttribute);

            return l.ReturnAndLog(newAttribute);
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
            _appWork.EntityMetadata(AppCtxWithDb).SaveMetadata(meta, AttributeMetadata.TypeGeneral, newValues);
            l.Done();
        }

        public bool UpdateInputType(int attributeId, string inputType)
        {
            var l = Log.Fn<bool>($"attrib:{attributeId}, input:{inputType}");
            var newValues = new Dictionary<string, object> { { AttributeMetadata.GeneralFieldInputType, inputType } };

            var meta = new Target((int)TargetTypes.Attribute, null, keyNumber: attributeId);
            // #ExtractEntitySave - verified
            _appWork.EntityMetadata(AppCtxWithDb).SaveMetadata(meta, AttributeMetadata.TypeGeneral, newValues);
            return l.ReturnTrue();
        }
    }
}
