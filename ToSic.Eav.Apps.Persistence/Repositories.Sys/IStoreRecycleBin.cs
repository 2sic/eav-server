using ToSic.Eav.Apps;

namespace ToSic.Eav.Repositories.Sys;

public interface IStoreRecycleBin: IServiceWithSetup<IAppReader>
{
    public record Parameters(
        DateTime? DateFrom = null,
        DateTime? DateTo = null,
        string? ContentType = null);

    IReadOnlyList<RecycleBinItem> Get(Parameters parameters);

    // TODO: @2rb - fix this, make it auto-convert etc.
    public sealed record RecycleBinItem(
        int Id,
        Guid Guid,
        int AppId,
        string TypeNameId,
        string TypeName,
        int TransactionId,
        DateTime Deleted,
        string? DeletedBy,
        string? ParentRef
    )
    {
        public string? Json { get; init; }
        public DateTime? FilterDateFrom { get; init; }
        public DateTime? FilterDateTo { get; init; }
        public string? FilterContentType { get; init; }
    };
}