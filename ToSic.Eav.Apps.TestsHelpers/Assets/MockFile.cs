using ToSic.Eav.Apps.Assets.Internal;

namespace ToSic.Eav.Apps.Assets;

public class MockFile: MockAsset, IFile
{
    public string Extension { get; init; } = "unk";
    public string Folder { get; init; } = "folder";
    public int FolderId { get; init; } = 0;
    public string FullName { get; init; } = "unknown.unk";
    public int Size { get; init; } = 0;
    public ISizeInfo SizeInfo => new SizeInfo(Size);
}