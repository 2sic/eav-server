namespace ToSic.Sys.Requirements;

/// <summary>
/// Marks objects which have requirements, which are necessary for further use/processing.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface IHasRequirements
{
    /// <summary>
    /// Requirements which are necessary for this feature to be used
    /// </summary>
    List<Requirement> Requirements { get; }
}