using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Models;

namespace ToSic.Eav.DataSources;

/// <summary>
/// Model of paging information, as used by the <see cref="Paging"/> DataSource.
/// </summary>
[ContentType(
    Guid = "488386e8-004c-4bd3-848c-46897835e6b1",  // Random new guid
    Description = "Paging Information",
    Name = "Paging"
)]
[PrivateApi] // #ToBeReleasedWithModels
public interface IPagingModel : IModelFromEntity<PagingModel>
{
    /// <summary>
    /// The page size - how many items are on a page.
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// The current page number - which page is currently shown.
    /// </summary>
    [ContentTypeField(IsTitle = true)]
    int PageNumber { get; }

    /// <summary>
    /// The total number of items in the source - not just the current page.
    /// </summary>
    int ItemCount { get; }

    /// <summary>
    /// The total number of pages - based on the page size and item count.
    /// </summary>
    int PageCount { get; }
}

/// <summary>
/// Model implementation of the IPagingModel.
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record PagingModel : ModelFromEntity, IPagingModel
{
    [field: AllowNull, MaybeNull]
    public string Title => field ??= Entity?.GetBestTitle() ?? "";
    public int PageSize => GetThis(0);
    public int PageNumber => GetThis(0);
    public int ItemCount => GetThis(0);
    public int PageCount => GetThis(0);
}

/// <summary>
/// Raw object to create paging entities.
/// </summary>
[ContentTypeUse(Type = typeof(IPagingModel))]
internal record PagingModelRaw(int PageSize, int PageNumber, int ItemCount, int PageCount) : IPagingModel, IRawEntityAutoConvert
{
    /// <summary>
    /// The ID should also contain the page number.
    /// </summary>
    public int Id => PageNumber;
}


