namespace ToSic.Eav.Data.Sys.Save;

/// <summary>
/// Opt-in revision of an existing published/draft pair. This is a save precondition, not authorization.
/// Both physical rows must still have these versions and the same logical identity.
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record ExistingDraftSaveCondition(
    int AppId,
    int EntityId,
    Guid EntityGuid,
    int PublishedVersion,
    int DraftId,
    int DraftVersion);
