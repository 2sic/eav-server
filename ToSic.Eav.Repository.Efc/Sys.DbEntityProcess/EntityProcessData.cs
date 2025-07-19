using ToSic.Eav.Data.Sys.Dimensions;
using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Save;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;

public record EntityProcessData
{
    public static EntityProcessData CreateInstance(IEntityPair<SaveOptions> entityOptionPair, bool logDetails)
        => new()
        {
            NewEntity = entityOptionPair.Entity,
            Options = entityOptionPair.Partner,
            LogDetails = logDetails,
            Progress = 0,
        };

    #region Foundation

    public required IEntity NewEntity { get; init; }
    public required SaveOptions Options { get; init; }
    public List<DimensionDefinition> Languages => Options.Languages ?? throw new ("languages missing in save-options. cannot continue");
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

    #region FinalResults

    public int FinalId => DbEntity!.EntityId;
    public Guid FinalGuid => DbEntity!.EntityGuid;

    #endregion

    public Exception? Exception { get; init; }


}

public static class EntityProcessDataExt
{
    public static ICollection<EntityProcessData> NextStep(this ICollection<EntityProcessData> list)
        => list
            .Select(d => d.NextStep())
            .ToListOpt();

    public static bool HasException(this ICollection<EntityProcessData> list, [NotNullWhen(true)] out Exception? firstException)
    {
        var first = list.FirstOrDefault(d => d.Exception != null);
        if (first == null)
        {
            firstException = null;
            return false;
        }

        firstException = first.Exception!;
        return true;
    }
}