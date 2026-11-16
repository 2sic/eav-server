using ToSic.Eav.Apps;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.Repositories.Sys;

public interface IStoreRecycleBin: IServiceWithSetup<IAppReader>
{
    public record Parameters(
        DateTime? DateFrom = null,
        DateTime? DateTo = null,
        string? ContentType = null);

    IReadOnlyList<RecycleBinItem> Get(Parameters parameters);

    [ContentType(
        Name = "RecycleBin",
        Guid = "0e451b14-b3f7-49f1-bb85-dfc17ba3d047",
        Description = "Deleted entity in the recycle bin",
        Scope = "System"
    )]
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
    ) : IRawEntityAutoConvert
    {
        [ContentTypeTitle]
        public string Title => $"{TypeName} ({Id})";

        public string? Json { get; init; }
    };
}
