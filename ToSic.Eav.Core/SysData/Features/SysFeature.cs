using ToSic.Eav.Plumbing;
using ToSic.Lib;

namespace ToSic.Eav.SysData;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public record SysFeature : Feature
{
    public const string Prefix = "System";


    public override required string NameId
    {
        get => base.NameId;
        init => base.NameId = EnsurePrefix(value);
    }
    private static string EnsurePrefix(string original) 
        => original.IsEmptyOrWs() ? Prefix + "-Error-No-Name" : original.StartsWith(Prefix) ? original : $"{Prefix}-{original}";

    public override string Link
    {
        get => field.UseFallbackIfNoValue(LibConstants.GoUrlSysFeats);
        init => field = value;
    }

    public override bool IsConfigurable => false;

    // 2025-05-06 2dm doesn't seem in use, disabled for now to move to central location
    //public IRawEntity RawEntity => _newEntity.Get(() => new RawEntity
    //{
    //    Guid = Guid,
    //    Values = new Dictionary<string, object>
    //    {
    //        {nameof(NameId), NameId},
    //        {nameof(Name), Name},
    //        {nameof(Description), Description},
    //        {nameof(Link), Link},
    //    }
    //});

    //private readonly GetOnce<IRawEntity> _newEntity = new();


    public override string ToString() => $"{Prefix}: {Name} ({NameId} / {Guid})";
}