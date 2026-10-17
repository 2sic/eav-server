using ToSic.Eav.Data;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.DataTests;

/// <summary>
/// Test some DTOs and verify that some fields should be object types (for conversion)
/// </summary>
/// <param name="contentTypes"></param>
[Startup(typeof(StartupTestsEavDataBuild))]
public class VerifyContentTypeFieldsOfTypeObject(ContentTypesFromCodeManager contentTypes)
{

    [Theory]
    [MemberData(nameof(ObjectFields))]
    public void CodeContentTypesDeclareObjectFields(Type type, string field)
        => Equal(ValueTypes.Object, contentTypes.Get(type)[field]!.Type);

    public static TheoryData<Type, string> ObjectFields => new()
    {
        { typeof(InputTypeInfoRaw), nameof(InputTypeInfoRaw.UiAssets) },
        { typeof(AppWebApiEndpointRaw), nameof(AppWebApiEndpointRaw.parameters) },
        { typeof(ContentTypeDto), nameof(ContentTypeDto.Properties) },
        { typeof(ContentTypeFieldDto), nameof(ContentTypeFieldDto.InputTypeConfig) },
        { typeof(DataSourceDto), nameof(DataSourceDto.Out) },
    };
}