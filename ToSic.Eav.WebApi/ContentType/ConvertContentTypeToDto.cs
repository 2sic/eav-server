using ToSic.Eav.Data.Shared;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.WebApi;

public class ConvertContentTypeToDto(LazySvc<IConvertToEavLight> convertToEavLight)
    : ServiceBase("Cnv.TypDto", connect: [convertToEavLight]), IConvert<IContentType, ContentTypeDto>
{
    public IEnumerable<ContentTypeDto> Convert(IEnumerable<IContentType> list)
    {
        var l = Log.Fn<IEnumerable<ContentTypeDto>>();
        var result = list.Select(i => Convert(i)).ToList();
        return l.Return(result, $"{result.Count}");
    }

    public ContentTypeDto Convert(IContentType item) => Convert(item, -1);

    public ContentTypeDto Convert(IContentType cType, int count)
    {
        var l = Log.Fn<ContentTypeDto>($"for json a:{cType.AppId}, type:{cType.Name}");

        // Note 2024-03-04 2dm - had errors with expired IServiceProvide getting deeper Metadata
        // This should just make it quiet, but there could be a deeper underlying issue.
        // Monitor - happened on Content App - but only when accessing scope System.Cms
        ContentTypeDetails details = null;
        try
        {
            details = cType.Metadata.DetailsOrNull;
        }
        catch (Exception ex)
        {
            l.E($"Getting details for content type '{cType}'");
            l.Ex(ex);
        }
        l.A("Got past retrieving metadata.");

        var nameOverride = details?.Title;
        if (string.IsNullOrEmpty(nameOverride))
            nameOverride = cType.Name;
        var ser = convertToEavLight.Value;

        var ancestorDecorator = cType.GetDecorator<IAncestor>();

        var properties = ser.Convert(details?.Entity);

        var jsonReady = new ContentTypeDto
        {
            Id = cType.Id,
            Name = cType.Name,
            Label = nameOverride,
            StaticName = cType.NameId,
            Scope = cType.Scope,
            Description = details?.Description,
            EditInfo = new(cType),
            UsesSharedDef = ancestorDecorator != null,
            SharedDefId = ancestorDecorator?.Id,
            Items = count,
            Fields = cType.Attributes.Count(),
            Metadata = (ser as ConvertToEavLight)?.CreateListOfSubEntities(cType.Metadata,
                SubEntitySerialization.AllTrue()),
            Properties = properties,
            Permissions = new() { Count = cType.Metadata.Permissions.Count() },
        };
        return l.ReturnAsOk(jsonReady);
    }
}