using ToSic.Lib;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Utils;

namespace ToSic.Sys.Capabilities.SysFeatures;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record SysFeature : Feature
{
    public const string Prefix = "System";


    public override required string NameId
    {
        get => base.NameId;
        init => base.NameId = EnsureNameIdPrefix(value);
    }
    
    /// <summary>
    /// Ensure that all these system features have a prefix; add if missing.
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    private static string EnsureNameIdPrefix(string original) 
        => original.IsEmptyOrWs() ? Prefix + "-Error-No-Name" : original.StartsWith(Prefix) ? original : $"{Prefix}-{original}";

    public override string Link
    {
        get => field.UseFallbackIfNoValue(LibConstants.GoUrlSysFeats);
        init;
    }

    public override bool IsConfigurable => false;

    public override string ToString() => $"{Prefix}: {Name} ({NameId} / {Guid})";
}