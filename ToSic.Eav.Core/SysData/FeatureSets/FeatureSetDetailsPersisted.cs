using System;

namespace ToSic.Eav.SysData;

public class FeatureSetDetailsPersisted
{
    /// <summary>
    /// A fingerprint / License
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Optional comments, like what system it's for
    /// </summary>
    public string Comments { get; set; }

    /// <summary>
    /// If parts of a license can expire, then it would be specified here.
    /// If it's null, it doesn't have an own expiry date.
    /// </summary>
    public DateTime? Expires { get; set; }
}