using ToSic.Eav.Data.Ancestors.Sys;
using ToSic.Eav.Data.ContentTypes.Sys;
using ToSic.Eav.Data.EntityDecorators.Sys;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.WebApi;

public class ConvertContentTypeToDto(LazySvc<IConvertToEavLight> convertToEavLight)
    : ServiceBase("Cnv.TypDto", connect: [convertToEavLight]), IConvert<IContentType, ContentTypeDto>
{
    public bool Debug { get; set; }

    public IEnumerable<ContentTypeDto> Convert(IEnumerable<IContentType> list)
    {
        var l = Log.Fn<IEnumerable<ContentTypeDto>>();
        var result = list.Select(i => Convert(i)).ToList();
        return l.Return(result, $"{result.Count}");
    }

    public ContentTypeDto Convert(IContentType item) => Convert(item, -1);

    private int _convertIteration = 0;

    public ContentTypeDto Convert(IContentType cType, int itemCount)
    {
        var l = Log.Fn<ContentTypeDto>($"for json app:{cType.AppId}, type:'{cType.Name}' / '{cType.NameId}'; iteration: {_convertIteration++}");

        // Note 2024-03-04 2dm - had errors with expired IServiceProvide getting deeper Metadata
        // This should just make it quiet, but there could be a deeper underlying issue.
        // Monitor - happened on Content App - but only when accessing scope System.Cms
        ContentTypeDetails details = null;
        try
        {
            details = cType.DetailsOrNull();
        }
        catch (Exception ex)
        {
            l.E($"Getting details for content type '{cType}'");
            l.Ex(ex);
            // 2024-05-16 2dm enabled throw again, as I believe I fixed #IServiceProviderDisposedException
            // Reason was that the EFC11 loader had a function which would retrieve a delayed metadata-source,
            // but it used an (unnecessary) API which needed DI.
            // Keep an eye on this for a while.
            throw;
        }
        l.A("Got past retrieving metadata.");

        var nameOverride = details?.Title;
        if (string.IsNullOrEmpty(nameOverride))
            nameOverride = cType.Name;
        var ser = convertToEavLight.Value;

        var ancestorDecorator = cType.GetDecorator<IAncestor>();

        var properties = ser.Convert(details?.Entity);

        var typeMetadata = cType.Metadata;

        var mdReferences = (ser as ConvertToEavLight)?
            .SubConverter
            .CreateListOfSubEntities(typeMetadata, SubEntitySerialization.NeverSerializeChildren());

        var permissionCount = typeMetadata.Permissions.Count();

        var jsonReady = new ContentTypeDto
        {
            Id = cType.Id,
            Name = cType.Name,
            Label = nameOverride,
            StaticName = cType.NameId,
            NameId = cType.NameId,
            Scope = cType.Scope,
            Description = details?.Description,
            EditInfo = new(cType),
            UsesSharedDef = ancestorDecorator != null,
            SharedDefId = ancestorDecorator?.Id,
            Items = itemCount,
            Fields = cType.Attributes.Count(),
            TitleField = cType.TitleFieldName,
            Metadata = mdReferences,
            Properties = properties,
            Permissions = new() { Count = permissionCount },
        };
        return l.ReturnAsOk(jsonReady);
    }
}