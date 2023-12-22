using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.WebApi.Dto;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.WebApi.Security;
using ToSic.Lib.DI;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.WebApi;

public class ConvertContentTypeToDto : ServiceBase, IConvert<IContentType, ContentTypeDto>
{
    private readonly LazySvc<IConvertToEavLight> _convertToEavLight;

    public ConvertContentTypeToDto(LazySvc<IConvertToEavLight> convertToEavLight) : base("Cnv.TypDto")
    {
        ConnectServices(
            _convertToEavLight = convertToEavLight
        );
    }

    public IEnumerable<ContentTypeDto> Convert(IEnumerable<IContentType> list)
    {
        var l = Log.Fn<IEnumerable<ContentTypeDto>>();
        var result = list.Select(i => Convert(i)).ToList();
        return l.Return(result, $"{result.Count}");
    }

    public ContentTypeDto Convert(IContentType item) => Convert(item, -1);

    public ContentTypeDto Convert(IContentType item, int count)
    {
        var l = Log.Fn<ContentTypeDto>($"for json a:{item.AppId}, type:{item.Name}");
        var details = item.Metadata.DetailsOrNull;

        var nameOverride = details?.Title;
        if (string.IsNullOrEmpty(nameOverride))
            nameOverride = item.Name;
        var ser = _convertToEavLight.Value;

        var ancestorDecorator = item.GetDecorator<IAncestor>();

        var properties = ser.Convert(details?.Entity);

        var jsonReady = new ContentTypeDto
        {
            Id = item.Id,
            Name = item.Name,
            Label = nameOverride,
            StaticName = item.NameId,
            Scope = item.Scope,
            Description = details?.Description,
            EditInfo = new EditInfoDto(item),
            UsesSharedDef = ancestorDecorator != null,
            SharedDefId = ancestorDecorator?.Id,
            Items = count,
            Fields = item.Attributes.Count(),
            Metadata = (ser as ConvertToEavLight)?.CreateListOfSubEntities(item.Metadata,
                SubEntitySerialization.AllTrue()),
            Properties = properties,
            Permissions = new HasPermissionsDto { Count = item.Metadata.Permissions.Count() },
        };
        return l.ReturnAsOk(jsonReady);
    }
}