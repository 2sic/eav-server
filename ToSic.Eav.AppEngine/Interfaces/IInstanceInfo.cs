namespace ToSic.Eav.Apps
{
    // TODO
    // probably also move to .Cms
    // Probably rename etc. - this is the container instance (module), not the SxcInstance
    public interface IInstanceInfo
    {
        int Id { get; }

        int PageId { get; }

        int TenantId { get; }

        bool IsPrimary { get; }
    }
}
