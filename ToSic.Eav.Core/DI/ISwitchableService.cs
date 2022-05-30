namespace ToSic.Eav.Plumbing.DI
{
    public interface ISwitchableService
    {
        string NameId { get; }

        bool IsViable();

        int Priority { get; }
    }
}
