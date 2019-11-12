namespace ToSic.Eav.Apps.Blocks
{
    // TODO
    // probably also move to .Blocks or .Cms
    // Probably rename etc. - this is the container instance (module), not the SxcInstance
    public interface ICmsBlock
    {
        int Id { get; }

        int PageId { get; }

        int TenantId { get; }

        bool IsPrimary { get; }
    }
}
