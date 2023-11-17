namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// Base class for requirements checkers.
    /// Just so we fully support the ISwitchable interface without having to code it in each checker
    /// </summary>
    public abstract class RequirementCheckBase: IRequirementCheck
    {
        public abstract string NameId { get; }

        public bool IsViable() => true;

        public int Priority => 0;

        public abstract bool IsOk(Condition condition);

        public abstract string InfoIfNotOk(Condition condition);
    }
}
