﻿using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Sys.Ancestors;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.ImportExport.Json.V1;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Recommendations.Sys;
using ToSic.Eav.Sys;
using ToSic.Eav.WebApi.Sys.Dto;
using static ToSic.Eav.Data.Sys.Attributes.AttributeMetadataConstants;


namespace ToSic.Eav.WebApi.Sys;

public class ConvertAttributeToDto(LazySvc<IConvertToEavLight> convertToLight, GenWorkPlus<WorkInputTypes> inputTypes, IAppReaderFactory appReaders, LazySvc<RecommendedMetadataService> mdRead)
    : ServiceBase("Cnv.AtrDto", connect: [inputTypes, convertToLight, mdRead]),
        IConvert<PairTypeWithAttribute, ContentTypeFieldDto>
{
    public ConvertAttributeToDto Init(int appId, bool withContentType)
    {
        var l = Log.Fn<ConvertAttributeToDto>($"{appId}, withContentType:{withContentType}");
        _appId = appId;
        _withContentType = withContentType;
        return l.Return(this);
    }

    private int _appId;
    private bool _withContentType;

    public IEnumerable<ContentTypeFieldDto> Convert(IEnumerable<PairTypeWithAttribute> list)
    {
        var l = Log.Fn<IEnumerable<ContentTypeFieldDto>>();
        var result = list
            .Select(Convert)
            .ToList();
        return l.Return(result, $"{result.Count}");
    }

    public ContentTypeFieldDto Convert(PairTypeWithAttribute? item)
    {
        var l = Log.Fn<ContentTypeFieldDto>();
        if (item == null)
            return l.ReturnNull("no item")!;

        var a = item.Attribute;
        var type = item.Type;
        var ancestorDecorator = type.GetDecorator<IAncestor>();
        var inputType = FindInputTypeOrUnknownOld(a);
        var appInputTypes = inputTypes.New(_appId).GetInputTypes()
            .OrderBy(it => it.Type) // order for easier debugging
            .ToList();
        var inputConfigs = GetInputTypesAndMetadata(inputType, a, type, ancestorDecorator, appInputTypes);

        // note: "ImageDecorator" is hardwired here, because it's a constant in 2sxc, not eav
        const string imageDecorator = "ImageDecorator";
        var isRecommended = IsRecommended(a.AttributeId.ToString(), imageDecorator);
        var imgDecorator = a.Metadata.FirstOrDefaultOfType(imageDecorator);

        var dto= new ContentTypeFieldDto
        {
            Id = a.AttributeId,
            SortOrder = a.SortOrder,
            Type = a.Type.ToString(),
            InputType = inputType,
            StaticName = a.Name,
            IsTitle = a.IsTitle,
            AttributeId = a.AttributeId,
            Metadata = inputConfigs.InputMetadata,
            InputTypeConfig = appInputTypes.FirstOrDefault(it => it.Type == inputType),
            Permissions = new()
            {
                Count = a.Metadata.Permissions.Count()
            },

            ImageConfiguration = new()
            {
                EntityId = imgDecorator?.EntityId ?? 0,
                IsRecommended = isRecommended,
                TypeName = imageDecorator
            },

            // new in 12.01
            IsEphemeral = a.Metadata.Get<bool>(MetadataFieldAllIsEphemeral, typeName: TypeGeneral),
            HasFormulas = a.HasFormulas(Log),

            // Read-Only new in v13
            EditInfo = new(type, a),

            // #SharedFieldDefinition
            Guid = a.Guid,
            SysSettings = JsonAttributeSysSettings.FromSysSettings(a.SysSettings),
            ContentType = _withContentType
                ? new JsonType(type, false, false)
                : null,

            // new 16.08
            ConfigTypes = inputConfigs.ConfigTypes,
        };

        return l.ReturnAsOk(dto);
    }

    private (IDictionary<string, bool> ConfigTypes, Dictionary<string, EavLightEntity> InputMetadata)
        GetInputTypesAndMetadata(string inputType, IContentTypeAttribute a, IContentType type, IAncestor? ancestorDecorator, IReadOnlyCollection<InputTypeInfo> appInputTypes)
    {
        var l = Log.Fn<(IDictionary<string, bool> ConfigTypes, Dictionary<string, EavLightEntity> InputMetadata)>();
        var configTypes = GetFieldConfigTypes(inputType, appInputTypes);

        // Note 2023-11-09 2dm - restricting what metadata is loaded - could have side effects
        var attribMetadata = (ContentTypeAttributeMetadata)a.Metadata;
        var mdToKeep = attribMetadata
            .Where(m => configTypes.Keys.Contains(m.Type.NameId) || configTypes.Keys.Contains(m.Type.Name))
            .ToList();

        l.A($"{nameof(mdToKeep)}: {mdToKeep.Count}; " + l.Try(() => string.Join(",", mdToKeep.Select(m => $"{m.Type}/{m}"))));

        var mdDeduplicated = mdToKeep
            .Select(e => new
            {
                Entity = e,
                TypeName = WorkInputTypes.GetTypeName(e.Type),
            })
            // Remove duplicates of metadata of the same type (would be faulty data, but can happen)
            .GroupBy(e => e.TypeName)
            .Select(grp => grp.First())
            .ToList();

        l.A($"{nameof(mdDeduplicated)}: {mdDeduplicated.Count}; " + l.Try(() => string.Join(",", mdDeduplicated.Select(m => $"{m.Entity.Type}/{m.Entity}"))));

        var inputMetadata = mdDeduplicated
            .ToDictionary(
                set => set.TypeName,
                set => InputMetadata(type, a, set.Entity, ancestorDecorator, convertToLight.Value)
            );


        // Do this after filtering the metadata
        configTypes = KeepOnlyConfigTypesWhichAreNotInherited(a, configTypes, Log);
        return l.Return((configTypes, inputMetadata));
    }


    private static IDictionary<string, bool> KeepOnlyConfigTypesWhichAreNotInherited(IContentTypeAttribute a, IDictionary<string, bool> configTypes, ILog log)
    {
        var l = log.Fn<IDictionary<string, bool>>($"{nameof(configTypes)} {configTypes.Count}");

        // Check if we're inheriting any metadata, as we don't want to give the 
        // inherited MD to the user to edit.
        var mdInheritList = a.SysSettings?.InheritMetadataOf?.ToList();
        if (mdInheritList.SafeNone()) return l.Return(configTypes, "no restrictions");

        var inheritExceptions = mdInheritList.Where(pair => pair.Key == Guid.Empty).ToList();

        l.A($"{nameof(inheritExceptions)}: {inheritExceptions.Count}");

        configTypes = configTypes
            .Where(ctPair => inheritExceptions.Any(ie => ie.Value == ctPair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        return l.Return(configTypes, $"{configTypes.Count}");
    }

    private EavLightEntity InputMetadata(IContentType contentType, IContentTypeAttribute a, IEntity e, IAncestor? ancestor, IConvertToEavLight ser)
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
        var inputType = attribute.Metadata.Get<string>(GeneralFieldInputType, typeName: TypeGeneral);

        // unknown will let the UI fallback on other mechanisms
        return inputType.IsEmpty()
            ? EavConstants.NullNameId
            : inputType;
    }

    /// <summary>
    /// Create a list of all expected types.
    /// Like for "@String-dropdown-query"
    /// - "@string"
    /// - "@string-dropdown"
    /// - "@string-dropdown-query"
    /// </summary>
    /// <param name="inputTypeName"></param>
    /// <param name="inputTypes"></param>
    /// <returns>Dictionary with name/required - ATM all required are set to true</returns>
    private IDictionary<string, bool> GetFieldConfigTypes(string inputTypeName, IReadOnlyCollection<InputTypeInfo> inputTypes)
    {
        var l = Log.Fn<IDictionary<string, bool>>();

        var inputType = FindInputType(inputTypeName);
        if (inputType == null)
            return l.Return(InputTypeInfo.NewDefaultConfigTypesDic(), "error - can't find type");

        // Get all types, like @All, @Entity, @entity-default
        // or if configured like @All, @string-picker
        var dicFromInfo = inputType.ConfigTypesDic();

        // Filer the ones that really exist for safety
        var finalDic = dicFromInfo
            .Where(pair => inputTypes.Any(i => i.Type.EqualsInsensitive(pair.Key.Trim('@'))))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        return l.Return(finalDic, $"{finalDic.Count}");

        InputTypeInfo? FindInputType(string name)
            => inputTypes.FirstOrDefault(i => i.Type.EqualsInsensitive(name));
    }

    #region Metadata Recommendations


    private bool IsRecommended(string targetIdentifier, string typeName)
    {
        var mdRecommendations = MdRecommendations();
        var recommendations= mdRecommendations.GetRecommendations((int)TargetTypes.Attribute, targetIdentifier);
        return recommendations.Any(r => r.Name == typeName);
    }

    private RecommendedMetadataService MdRecommendations()
    {
        if (_mdRecs != null)
            return _mdRecs;
        _mdRecs = mdRead.Value;
        var appReader = appReaders.Get(_appId);
        _mdRecs.Setup(appReader, _appId);
        return _mdRecs;
    }
    private RecommendedMetadataService? _mdRecs;

    #endregion
}