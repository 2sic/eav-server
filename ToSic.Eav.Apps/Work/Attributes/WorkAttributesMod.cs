using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using System.Linq;

namespace ToSic.Eav.Apps.Work
{
    public class WorkAttributesMod : WorkUnitBase<IAppWorkCtxWithDb>
    {
        private readonly ContentTypeAttributeBuilder _attributeBuilder;
        private readonly GenWorkDb<WorkMetadata> _workMetadata;

        public WorkAttributesMod(GenWorkDb<WorkMetadata> workMetadata, ContentTypeAttributeBuilder attributeBuilder) : base("ApS.InpGet")
        {
            ConnectServices(
                _attributeBuilder = attributeBuilder,
                _workMetadata = workMetadata
            );
        }

        #region Getters which don't modify, but need the DB

        /// <summary>
        /// Get all known data types, eg "String", "Number" etc. from DB.
        /// It should actually not be in the ...Mod because it doesn't modify anything, but it's here because it needs the DB.
        /// </summary>
        /// <returns></returns>
        public string[] DataTypes()
        {
            var l = Log.Fn<string[]>();
            var result = AppWorkCtx.DataController.Attributes.DataTypeNames();
            return l.Return(result, $"{result?.Length ?? 0}");
        }

        #endregion

        #region Add Field

        public int AddField(int contentTypeId, string staticName, string type, string inputType, int sortOrder)
        {
            var l = Log.Fn<int>($"add field type#{contentTypeId}, name:{staticName}, type:{type}, input:{inputType}, order:{sortOrder}");
            var attDef = _attributeBuilder
                .Create(appId: AppWorkCtx.AppId, name: staticName, type: ValueTypeHelpers.Get(type), isTitle: false, id: 0, sortOrder: sortOrder);
            var id = AddField(contentTypeId, attDef, inputType);
            return l.Return(id);
        }


        /// <summary>
        /// Append a new Attribute to an AttributeSet
        /// Simple overload returning int so it can be used from outside
        /// </summary>
        private int AddField(int attributeSetId, ContentTypeAttribute attDef, string inputType)
        {
            var l = Log.Fn<int>($"type:{attributeSetId}, input:{inputType}");
            var newAttribute = AppWorkCtx.DataController.Attributes.AddAttributeAndSave(attributeSetId, attDef);

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
            _workMetadata.New(AppWorkCtx).SaveMetadata(meta, AttributeMetadata.TypeGeneral, newValues);
            l.Done();
        }

        #endregion

        #region Changes to input type, name, etc.

        public bool SetInputType(int attributeId, string inputType)
        {
            var l = Log.Fn<bool>($"attrib:{attributeId}, input:{inputType}");
            var newValues = new Dictionary<string, object> { { AttributeMetadata.GeneralFieldInputType, inputType } };

            var meta = new Target((int)TargetTypes.Attribute, null, keyNumber: attributeId);
            _workMetadata.New(AppWorkCtx).SaveMetadata(meta, AttributeMetadata.TypeGeneral, newValues);
            return l.ReturnTrue();
        }

        public bool Rename(int contentTypeId, int attributeId, string newName)
        {
            var l = Log.Fn<bool>($"rename attribute type#{contentTypeId}, attrib:{attributeId}, name:{newName}");
            AppWorkCtx.DataController.Attributes.RenameAttribute(attributeId, contentTypeId, newName);
            return l.ReturnTrue();
        }

        public bool Reorder(int contentTypeId, string orderCsv)
        {
            var l = Log.Fn<bool>($"reorder type#{contentTypeId}, order:{orderCsv}");
            var sortOrderList = orderCsv.Split(',').Select(int.Parse).ToList();
            AppWorkCtx.DataController.ContentType.SortAttributes(contentTypeId, sortOrderList);
            return l.ReturnTrue();
        }


        public bool Delete(int contentTypeId, int attributeId)
        {
            var l = Log.Fn<bool>($"delete field type#{contentTypeId}, attrib:{attributeId}");
            return l.Return(AppWorkCtx.DataController.Attributes.RemoveAttributeAndAllValuesAndSave(attributeId));
        }


        #endregion


    }
}
