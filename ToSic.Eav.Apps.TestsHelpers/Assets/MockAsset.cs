namespace ToSic.Eav.Apps.Assets;

public class MockAsset: IAsset
{
    public DateTime Created { get; init; }
    public int Id { get; init; }
    public int ParentId { get; init; }
    public DateTime Modified { get; init; }
    public string Name { get; init; } = "unknown";
    public string Path { get; init; } = "/folder";
    public string PhysicalPath { get; init; } = "c:\\dummy\\folder";
}