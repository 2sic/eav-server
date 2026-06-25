using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Models;

namespace ToSic.Eav.DataSources;

[ModelSpecs(Use = typeof(PagingModelOfEntity))]
[PrivateApi]
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
internal record PagingModel(int PageSize, int PageNumber, int ItemCount, int PageCount) : RawEntityRecordBase, IPagingModel
{
    public string Title => "Paging Information";
    public override int Id => PageNumber;

    public override IDictionary<string, object?> Attributes(RawConvertOptions options) =>
        new Dictionary<string, object?>
        {
            { AttributeNames.TitleNiceName, Title },
            { nameof(PageSize), PageSize },
            { nameof(PageNumber), PageNumber },
            { nameof(ItemCount), ItemCount },
            { nameof(PageCount), PageCount }
        };
}


