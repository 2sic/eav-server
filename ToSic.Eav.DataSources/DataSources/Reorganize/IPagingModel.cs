using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
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

[ContentTypeSpecs(
    Guid = "488386e8-004c-4bd3-848c-46897835e6b1",
    Description = "Paging Information",
    Name = "Paging"
)]
internal record PagingModel(int PageSize, int PageNumber, int ItemCount, int PageCount) : /*RawEntityRecordBase,*/ IPagingModel, IGetRawConverter
{
    public string Title => "Paging Information";
    //public override int Id => PageNumber;

    //private IDictionary<string, object?> Values => new Dictionary<string, object?>
    //{
    //    { AttributeNames.TitleNiceName, Title },
    //    { nameof(PageSize), PageSize },
    //    { nameof(PageNumber), PageNumber },
    //    { nameof(ItemCount), ItemCount },
    //    { nameof(PageCount), PageCount }
    //};

    //public override IDictionary<string, object?> Attributes(RawConvertOptions options) => Values;

    IConvertToRawEntity IGetRawConverter.GetConverter() => Converter;

    private static IConvertToRawEntity Converter { get; } = new ConvertToRawFactory<PagingModel>((source, options) =>
        new RawEntityRecord
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


