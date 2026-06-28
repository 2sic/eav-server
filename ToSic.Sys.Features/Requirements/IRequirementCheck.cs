namespace ToSic.Sys.Requirements;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IRequirementCheck: ISwitchableService
{
    bool IsOk(Requirement requirement);

    string InfoIfNotOk(Requirement requirement);

    RequirementStatus Status(Requirement requirement);
}