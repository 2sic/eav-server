using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

public record EntityProcessData
{
    #region Foundation

    public required IEntity NewEntity { get; init; }
    public required SaveOptions Options { get; init; }
    public required bool LogDetails { get; init; }
    public required int Progress { get; init; }

    public EntityProcessData NextStep() => this with { Progress = Progress + 1 };

    #endregion

    #region Results from Level 1

    public bool SaveJson { get; init; }

    #endregion

    #region Results from Level 2

    public bool IsNew { get; init; }
    public int? ExistingDraftId { get; init; }
    public bool HasAdditionalDraft { get; init; }
    public int ContentTypeId { get; init; }
    public List<TsDynDataAttribute> AttributeDefs { get; init; }

    #endregion

    #region Results from Level 3

    public TsDynDataEntity? DbEntity { get; init; }

    // 3a
    public string? JsonExport { get; init; }

    // 3b
    public bool StateChanged { get; init; }

    #endregion



    public Exception? Exception { get; init; }


}