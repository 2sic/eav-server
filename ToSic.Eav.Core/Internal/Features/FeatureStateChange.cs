using System;

namespace ToSic.Eav.Internal.Features;

public class FeatureManagementChange
{
    public Guid FeatureGuid { get; set; }
        
    /// <summary>
    /// Feature can be enabled, disabled or null.
    /// Null feature are removed from features stored.
    /// </summary>
    public bool? Enabled { get; set; }
}