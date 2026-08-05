using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Models;

namespace ToSic.Eav.DataSources;

[ContentType(
    Guid = "488386e8-004c-4bd3-848c-46897835e6b1",
    Description = "Paging Information",
    Name = "Paging"
)]
[PrivateApi] // #ToBeReleasedWithModels
public interface IPagingModel : IModelFromEntity<PagingModel>
{
    //string Title { get; }
    int PageSize { get; }

    [ContentTypeField(IsTitle = true)]
    int PageNumber { get; }
    int ItemCount { get; }
    int PageCount { get; }
}

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record PagingModel : ModelFromEntity, IPagingModel
{
    [field: AllowNull, MaybeNull]
    public string Title => field
        ??= Entity?.GetBestTitle() ?? "";

    public int PageSize => GetThis(0);
    public int PageNumber => GetThis(0);
    public int ItemCount => GetThis(0);
    public int PageCount => GetThis(0);
}

[ContentTypeUse(Type = typeof(IPagingModel))]
internal record PagingModelRaw(int PageSize, int PageNumber, int ItemCount, int PageCount) : IPagingModel, IRawEntityConvertible
{
    //public string Title => "Paging Information";

    IRawEntityConverter IRawEntityConvertible.GetConverter() => Converter;

    private static IRawEntityConverter Converter { get; } = new RawEntityConverterFactory<PagingModelRaw>((source, _) =>
        new RawEntity
        {
            Id = source.PageNumber,
            Values = new Dictionary<string, object?>
            {
                //{ AttributeNames.TitleNiceName, source.Title },
                { nameof(PageSize), source.PageSize },
                { nameof(PageNumber), source.PageNumber },
                { nameof(ItemCount), source.ItemCount },
                { nameof(PageCount), source.PageCount }
            },
        });
}


