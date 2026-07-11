using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories;

internal record MockRawEntityRecord(
    IDictionary<string, object?> Values)
    : IRawEntity
{
    public const int DefaultId = 42;

    public int Id { get; init; } = DefaultId;
    public Guid Guid { get; init; } = Guid.NewGuid();
    public DateTime Created { get; init; } = DateTime.Now;
    public DateTime Modified { get; init; } = DateTime.Now;
};