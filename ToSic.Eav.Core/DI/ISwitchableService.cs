namespace ToSic.Eav.DI
{
    public interface ISwitchableService
    {
        string NameId { get; }

        bool IsViable();

        int Priority { get; }
    }
}
