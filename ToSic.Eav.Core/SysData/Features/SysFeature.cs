using ToSic.Eav.Data.Raw;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Coding;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.SysData;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class SysFeature(string nameId, Guid guid, string name, string description = default, string link = default)
    : Feature(EnsurePrefix(nameId), guid, name, isPublic: false, ui: false, description, FeatureSecurity.Unknown,
        BuiltInFeatures.SystemEnabled), IHasRawEntity<IRawEntity>
{
    public const string Prefix = "System";

    /// <summary>
    /// Clone constructor - optionally override some values
    /// </summary>
    public SysFeature Clone(NoParamOrder noParamOrder = default, 
        string nameId = default, Guid? guid = default, string name = default, string description = default, string link = default) =>
        new(nameId ?? NameId, guid ?? Guid, name ?? Name, description ?? Description, link ?? Link);

    private static string EnsurePrefix(string original) 
        => original.IsEmptyOrWs() ? Prefix + "-Error-No-Name" : original.StartsWith(Prefix) ? original : $"{Prefix}-{original}";

    public override string Link => link.UseFallbackIfNoValue(Constants.GoUrlSysFeats);

    public override bool IsConfigurable => false;

    public IRawEntity RawEntity => _newEntity.Get(() => new RawEntity
    {
        Guid = Guid,
        Values = new Dictionary<string, object>
        {
            {nameof(NameId), NameId},
            {nameof(Name), Name},
            {nameof(Description), Description},
            {nameof(Link), Link},
        }
    });

    private readonly GetOnce<IRawEntity> _newEntity = new();


    public override string ToString() => $"{Prefix}: {Name} ({NameId} / {Guid})";
}