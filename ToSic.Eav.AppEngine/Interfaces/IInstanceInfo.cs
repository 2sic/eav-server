namespace ToSic.Eav.Apps.Interfaces
{
    public interface IInstanceInfo
    {
        int Id { get; }

        int PageId { get; }

        int TenantId { get; }

        bool IsPrimary { get; }
    }
}
