using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Models;

namespace ToSic.Eav.DataSources;

[ModelSpecs(Use = typeof(PagingModelOfEntity))]
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IPagingModel : IModelFromData
{
    string Title { get; }
    int PageSize { get; }
    int PageNumber { get; }
    int ItemCount { get; }
    int PageCount { get; }
}

[PrivateApi]
internal record PagingModelOfEntity : ModelFromEntityBasic, IPagingModel
{
    public int PageSize => GetThis(0);
    public int PageNumber => GetThis(0);
    public int ItemCount => GetThis(0);
    public int PageCount => GetThis(0);
}

[ContentType(
    Guid = "488386e8-004c-4bd3-848c-46897835e6b1",
    Description = "Paging Information",
    Name = "Paging"
)]
internal record PagingModel(int PageSize, int PageNumber, int ItemCount, int PageCount) : IPagingModel, IRawEntityConvertible
{
    public string Title => "Paging Information";

    IRawEntityConverter IRawEntityConvertible.GetConverter() => Converter;

    private static IRawEntityConverter Converter { get; } = new RawEntityConverterFactory<PagingModel>((source, _) =>
        new RawEntity
        {
            Id = source.PageNumber,
            Values = new Dictionary<string, object?>
            {
                { AttributeNames.TitleNiceName, source.Title },
                { nameof(PageSize), source.PageSize },
                { nameof(PageNumber), source.PageNumber },
                { nameof(ItemCount), source.ItemCount },
                { nameof(PageCount), source.PageCount }
            },
        });
}


