using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Requirements;

public class RequirementStatus(bool isOk, Aspect aspect = default, string message = default)
{
    public bool IsOk { get; } = isOk;

    public Aspect Aspect { get; } = aspect;

    public string Message { get; } = message;
}