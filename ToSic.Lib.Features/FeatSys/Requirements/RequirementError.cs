using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Requirements;

public class RequirementError(Requirement requirement, string message)
{
    public Requirement Requirement { get; set; } = requirement;

    public string Message { get; set; } = message;
}