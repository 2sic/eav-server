using System.Text.Json;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Sys;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.FeatureSet;

namespace ToSic.Eav.DataSources.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
internal abstract class FeatureStateRawBase(FeatureState state)
{
    protected FeatureState State { get; } = state;

    public Guid Guid => State.Aspect.Guid;
    public string NameId => State.NameId;

    [ContentTypeTitle]
    public string Name => State.Aspect.Name;

    public string Description => State.Aspect.Description;
    public bool IsEnabled => State.IsEnabled;
    public bool AllowUse => State.IsEnabled;
    public string Behavior => State.Aspect.DisabledBehavior.ToString().ToLowerInvariant();
    public string Link => State.Aspect.Link;
    public string LicenseName => State.License?.Name ?? EavConstants.NullNameId;
    public Guid LicenseGuid => State.License?.Guid ?? Guid.Empty;
}

internal sealed class FeatureStateMinimalRaw(FeatureState state)
    : FeatureStateRawBase(state), IRawEntityAutoConvert;

internal class FeatureStateRaw(FeatureState state)
    : FeatureStateRawBase(state), IRawEntityAutoConvert
{
    public bool EnabledByDefault => State.EnabledByDefault;
    public bool? EnabledInConfiguration => State.EnabledInConfiguration;
    public DateTime Expiration => State.Expiration;
    public bool IsForEditUi => State.IsForEditUi;
    public bool AllowedByLicense => State.AllowedByLicense;
    public bool IsPublic => State.IsPublic;
}

[ContentType(
    Name = "FeatureState",
    Guid = "456b1bf2-74c4-4832-9d5b-be1e86b87da5" // Random
)]
internal sealed class FeatureStateDetailedRaw(FeatureState state)
    : FeatureStateRaw(state)
{
    public string EnabledReason => State.EnabledReason;
    public string EnabledReasonDetailed => State.EnabledReasonDetailed;
    public bool IsConfigurable => State.Aspect.IsConfigurable;
    public string Configuration => JsonSerializer.Serialize(State.Configuration);
    public string? ConfigurationContentType => State.Aspect.ConfigurationContentType;
    public int SecurityImpact => State.Security.Impact;
    public string SecurityMessage => State.Security.Message;
}

/// <summary>
/// Information about a license and its current state.
/// </summary>
[ContentType(
    Name="Licenses",
    Guid = "bf4e0072-6b05-4e4b-94a9-8474ec04919c" // Random
)]
internal sealed class FeatureSetStateRaw(FeatureSetState state) : IRawEntityAutoConvert
{
    public Guid Guid => state.Aspect.Guid;

    [ContentTypeTitle]
    public string Name => state.Aspect.Name;

    public string NameId => state.Aspect.NameId;
    public string? LicenseKey => state.LicenseKey;
    public string Description => state.Aspect.Description;
    public bool AutoEnable => state.Aspect.AutoEnable;
    public int Priority => state.Aspect.Priority;
    public bool FeatureLicense => state.Aspect.FeatureLicense;
    public bool IsEnabled => state.IsEnabled;
    public bool EnabledInConfiguration => state.EnabledInConfiguration;
    public bool Valid => state.Valid;
    public DateTime Expiration => state.Expiration;
    public bool ExpirationIsValid => state.ExpirationIsValid;
    public bool SignatureIsValid => state.SignatureIsValid;
    public bool FingerprintIsValid => state.FingerprintIsValid;
    public bool VersionIsValid => state.VersionIsValid;
    public string? Owner => state.Owner;
}