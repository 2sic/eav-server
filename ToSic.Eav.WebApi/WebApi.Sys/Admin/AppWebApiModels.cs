using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.WebApi.Sys.ApiExplorer;

namespace ToSic.Eav.WebApi.Sys.Admin;

[ContentTypeSpecs(
    Name = "ApiFile", 
    Guid = "98e35962-ae3c-44b3-a3fd-1275419825c7", 
    Description = "App WebApi controller file", 
    Scope = "System"
    )]
public class AppWebApiFileModel(AllApiFileDto file) : RawEntity
{
    [ContentTypeAttributeSpecs(IsTitle = true)] public string Path => file.Path;
    public override IDictionary<string, object?> Attributes(RawConvertOptions options) => new Dictionary<string, object?>
    {
        { nameof(AllApiFileDto.Path), file.Path }, { nameof(AllApiFileDto.EndpointPath), file.EndpointPath },
        { nameof(AllApiFileDto.Edition), file.Edition }, { nameof(AllApiFileDto.Shared), file.Shared },
    };
}

[ContentTypeSpecs(
    Name = "AppWebApiControllerDetails", 
    Guid = "70179265-0a90-4605-953a-91d237bed938", 
    Description = "App WebApi controller details", 
    Scope = "System"
    )]
internal class AppWebApiControllerModel(ApiControllerDto controller) : RawEntity
{
    [ContentTypeAttributeSpecs(IsTitle = true)] public string Name => controller.controller;
    public override IDictionary<string, object?> Attributes(RawConvertOptions options)
    {
        var values = AppWebApiControllerSecurityValues.ToDictionary(controller.security);
        values.Add(nameof(ApiControllerDto.controller), controller.controller);
        return values;
    }
}

[ContentTypeSpecs(
    Name = "AppWebApiControllerEndpoint", 
    Guid = "06efd171-7d8f-4752-8ced-e444c8247c70", 
    Description = "App WebApi controller endpoint", 
    Scope = "System"
    )]
public class AppWebApiEndpointModel(ApiActionDto action) : RawEntity
{
    [ContentTypeAttributeSpecs(IsTitle = true)] public string Name => action.name;
    public override IDictionary<string, object?> Attributes(RawConvertOptions options)
    {
        var values = AppWebApiControllerSecurityValues.ToDictionary(action.mergedSecurity);
        values.Add(nameof(ApiActionDto.name), action.name);
        values.Add(nameof(ApiActionDto.returns), action.returns);
        values.Add(nameof(ApiActionDto.verbs), string.Join(", ", action.verbs));
        values.Add(nameof(ApiActionDto.parameters), action.parameters);
        values.Add(nameof(ApiActionDto.security), AppWebApiControllerSecurityValues.ToDictionary(action.security));
        return values;
    }
}