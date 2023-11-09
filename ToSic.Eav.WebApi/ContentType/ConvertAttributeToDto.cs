using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.WebApi.Dto;
using ToSic.Eav.WebApi.Security;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using ToSic.Eav.Apps.Work;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Logging;


namespace ToSic.Eav.WebApi
{
    public class ConvertAttributeToDto: ServiceBase, IConvert<PairTypeWithAttribute, ContentTypeFieldDto>
    {
        private readonly GenWorkPlus<WorkInputTypes> _inputTypes;
        private readonly LazySvc<IConvertToEavLight> _convertToEavLight;

        public ConvertAttributeToDto(LazySvc<IConvertToEavLight> convertToEavLight, GenWorkPlus<WorkInputTypes> inputTypes) : base("Cnv.AtrDto")
        {
            ConnectServices(
                _inputTypes = inputTypes,
                _convertToEavLight = convertToEavLight
            );
        }

        public ConvertAttributeToDto Init(int appId, bool withContentType)
        {
            _appId = appId;
            _withContentType = withContentType;
            return this;
        }

        private int _appId;
        private bool _withContentType;

        public IEnumerable<ContentTypeFieldDto> Convert(IEnumerable<PairTypeWithAttribute> list)
        {
            var l = Log.Fn<IEnumerable<ContentTypeFieldDto>>();
            var result = list.Select(Convert).ToList();
            return l.Return(result, $"{result.Count}");
        }

        public ContentTypeFieldDto Convert(PairTypeWithAttribute item)
        {
            var l = Log.Fn<ContentTypeFieldDto>();
            if (item == null) return l.ReturnNull("no item");

            var a = item.Attribute;
            var type = item.Type;
            var ancestorDecorator = type.GetDecorator<IAncestor>();
            var inputType = FindInputTypeOrUnknownOld(a);
            var ser = _convertToEavLight.Value;
            var appInputTypes = _inputTypes.New(_appId).GetInputTypes();
            var configTypes = GetFieldConfigTypes(inputType, appInputTypes);

            // TODO: CONTINUE HERE - ONLY KEEP METADATA ACCORDING TO CONFIG-TYPES
            // Note 2023-11-09 2dm - restricting what metadata is loaded - could have side-effects
            var mdToKeep = a.Metadata.Where(m => configTypes.Keys.Contains(m.Type.NameId)).ToList();

            var inputMetadata = mdToKeep // a.Metadata
                .ToDictionary(
                    e => (Guid.TryParse(e.Type.NameId, out _) ? e.Type.Name : e.Type.NameId).TrimStart('@'),
                    e => InputMetadata(type, a, e, ancestorDecorator, ser));

            var dto= new ContentTypeFieldDto
            {
                Id = a.AttributeId,
                SortOrder = a.SortOrder,
                Type = a.Type.ToString(),
                InputType = inputType,
                StaticName = a.Name,
                IsTitle = a.IsTitle,
                AttributeId = a.AttributeId,
                Metadata =inputMetadata,
                InputTypeConfig = appInputTypes.FirstOrDefault(it => it.Type == inputType),
                Permissions = new HasPermissionsDto { Count = a.Metadata.Permissions.Count() },

                // new in 12.01
                IsEphemeral = a.Metadata.GetBestValue<bool>(AttributeMetadata.MetadataFieldAllIsEphemeral,
                    AttributeMetadata.TypeGeneral),
                HasFormulas = a.HasFormulas(Log),

                // Read-Only new in v13
                EditInfo = new EditInfoAttributeDto(type, a),

                // #SharedFieldDefinition
                Guid = a.Guid,
                SysSettings = JsonAttributeSysSettings.FromSysSettings(a.SysSettings),
                ContentType = _withContentType ? new JsonType(type, false, false) : null,

                // new 16.08
                ConfigTypes = configTypes,
            };

            return l.ReturnAsOk(dto);
        }

        private EavLightEntity InputMetadata(IContentType contentType, IContentTypeAttribute a, IEntity e, IAncestor ancestor, IConvertToEavLight ser)
        {
            var result = ser.Convert(e);
            if (ancestor != null)
                result.Add("IdHeader", new
                {
                    e.EntityId,
                    Ancestor = true,
                    IsMetadata = true,
                    OfContentType = contentType.NameId,
                    OfAttribute = a.Name,
                });

            return result;
        }

        /// <summary>
        /// The old method, which returns the text "unknown" if not known. 
        /// As soon as the new UI is used, this must be removed / deprecated
        /// TODO: 2023-11 @2dm - this seems very old and has a note that it should be removed on new UI, but I'm not sure if this has already happened
        /// TODO: should probably check if the UI still does any "unknown" checks
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// It's important to NOT cache this result, because it can change during runtime, and then a cached info would be wrong. 
        /// </remarks>
        private static string FindInputTypeOrUnknownOld(IContentTypeAttribute attribute)
        {
            var inputType = attribute.Metadata.GetBestValue<string>(AttributeMetadata.GeneralFieldInputType, AttributeMetadata.TypeGeneral);

            // unknown will let the UI fallback on other mechanisms
            return string.IsNullOrEmpty(inputType) ? Constants.NullNameId : inputType;
        }

        /// <summary>
        /// Create a list of all expected types.
        /// Eg. for "@String-dropdown-query"
        /// - "@string"
        /// - "@string-dropdown"
        /// - "@string-dropdown-query"
        /// </summary>
        /// <param name="inputTypeName"></param>
        /// <param name="inputTypes"></param>
        /// <returns></returns>
        private IDictionary<string, bool> GetFieldConfigTypes(string inputTypeName, IReadOnlyCollection<InputTypeInfo> inputTypes)
        {
            var l = Log.Fn<IDictionary<string, bool>>();

            InputTypeInfo FindInputType(string name) => inputTypes.FirstOrDefault(i => i.Type.EqualsInsensitive(name));

            var inputType = FindInputType(inputTypeName);
            if (inputType == null)
                return l.Return(new Dictionary<string, bool> { [AttributeMetadata.TypeGeneral] = true }, "error - can't find type");

            var dicFromInfo = inputType.ConfigTypes();

            var finalDic = dicFromInfo
                .Where(pair => inputTypes.Any(i => i.Type.EqualsInsensitive(pair.Key.Trim('@'))))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return l.Return(finalDic, $"{finalDic.Count}");
        }
    }
}
