namespace ToSic.Eav.Apps.Assets;

public class MockFolder: MockAsset, IFolder
{
    public bool HasChildren { get; init; }
}