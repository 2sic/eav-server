using ToSic.Eav.Metadata;

namespace ToSic.Eav.Models.TestData;

/// <summary>
/// The interface - which when used in ToModel will automatically use the <see cref="MockMetadataModel"/>.
/// </summary>
internal interface IMockMetadataModel : IModelFromEntity<MockMetadataModel>
{
    public int TargetType { get; }
    public string TargetName { get; }
    public int Amount { get; }
    public string? DeleteWarning { get; }
}