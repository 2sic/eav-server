namespace ToSic.Eav.Data.Raw.Sys;

public class RawEntityConstants
{
    public static readonly string[] KeysToRemove =
    [
        nameof(IRawEntity.Id),
        nameof(IRawEntity.Guid),
        nameof(IRawEntity.Created),
        nameof(IRawEntity.Modified),
    ];

}
