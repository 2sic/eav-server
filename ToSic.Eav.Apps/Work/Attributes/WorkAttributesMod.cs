﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Metadata;
using ToSic.Eav.Serialization;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Work
{
    public class WorkAttributesMod : WorkUnitBase<IAppWorkCtxWithDb>
    {
        private readonly ContentTypeAttributeBuilder _attributeBuilder;
        private readonly GenWorkDb<WorkMetadata> _workMetadata;
        private readonly Generator<IDataDeserializer> _dataDeserializer;

        public WorkAttributesMod(GenWorkDb<WorkMetadata> workMetadata, ContentTypeAttributeBuilder attributeBuilder, Generator<IDataDeserializer> dataDeserializer) : base("ApS.InpGet")
        {

            ConnectServices(
                _attributeBuilder = attributeBuilder,
                _workMetadata = workMetadata,
                _dataDeserializer = dataDeserializer
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

        #region New Sharing Features

        public void FieldShare(int attributeId, bool share, bool hide = false)
        {
            var l = Log.Fn($"attributeId:{attributeId}, share:{share}, hide:{hide}");

            // get field attributeId
            var attribute = AppWorkCtx.DataController.Attributes.Get(attributeId)
                /*?? throw new ArgumentException($"Attribute with id {attributeId} does not exist.")*/;

            // update with the Share = share (hide we'll ignore for now, it's for future needs)
            var newSysSettings = new ContentTypeAttributeSysSettings(share: share);

            var serializer = _dataDeserializer.New();
            serializer.Initialize(AppWorkCtx.AppId, new List<IContentType>(), null);

            // Update DB, and then flush the app-cache as necessary, same as any other attribute change
            AppWorkCtx.DataController.DoAndSave(() =>
            {
                // ensure GUID: update the field definition in the DB to ensure it has a GUID (but don't change if it already has one)
                if (attribute.Guid.HasValue == false) attribute.Guid = Guid.NewGuid();

                attribute.SysSettings = serializer.Serialize(newSysSettings);
            });

            l.Done();
        }

        public void FieldInherit(int attributeId, Guid inheritMetadataOf)
        {
            var l = Log.Fn($"attributeId:{attributeId}, inheritMetadataOf:{inheritMetadataOf}");

            // get field attributeId
            var attribute = AppWorkCtx.DataController.Attributes.Get(attributeId);

            // set InheritMetadataOf to the guid above(as string)
            var newSysSettings = new ContentTypeAttributeSysSettings(
                inherit: null,
                inheritName: false,
                inheritMetadata: false,
                inheritMetadataOf: new Dictionary<Guid, string>() { [inheritMetadataOf] = "" });

            var serializer = _dataDeserializer.New();
            serializer.Initialize(AppWorkCtx.AppId, new List<IContentType>(), null);

            // Update DB, and then flush the app-cache as necessary, same as any other attribute change
            AppWorkCtx.DataController.DoAndSave(() => attribute.SysSettings = serializer.Serialize(newSysSettings));

            l.Done();
        }

        #endregion
    }
}
