using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Requirements;

/// <summary>
/// Base class for requirements checkers.
/// Just so we fully support the ISwitchable interface without having to code it in each checker
/// </summary>
public abstract class RequirementCheckBase: IRequirementCheck
{
    public abstract string NameId { get; }

    public bool IsViable() => true;

    public int Priority => 0;

    public abstract bool IsOk(Requirement requirement);

    public abstract string InfoIfNotOk(Requirement requirement);

    protected abstract Aspect GetAspect(Requirement requirement);

    public virtual RequirementStatus Status(Requirement requirement)
    {
        var ok = IsOk(requirement);
        var aspect = GetAspect(requirement);
        var message = ok ? null : InfoIfNotOk(requirement);
        return new(ok, aspect, message);
    }
}