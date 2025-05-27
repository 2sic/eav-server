using ToSic.Eav.Security.Encryption;
using ToSic.Lib.Helpers;

namespace ToSic.Sys.Capabilities.Fingerprints;

/// <summary>
/// This is for fingerprints which are not auto-generated,
/// but instead are provided by data - usually through license registration.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class EnterpriseFingerprint
{
    public required int Id { get; init; }
    public required Guid Guid { get; init; }
    public required string Title { get; init; }
    public required string Fingerprint { get; init; }
    public bool Valid => ValidityInternal.Valid;
    public string ValidityMessage => ValidityInternal.Message;


    private (bool Valid, string Message) ValidityInternal => _validity.Get(() =>
    {
        var signatureOriginal = $"{Guid};{Title}";
        var expected = Sha256.Hash(signatureOriginal);
        var ok = expected == Fingerprint;
        return ok
            ? (true, $"License ok; For: '{signatureOriginal}'")
            : (false, $"Fingerprint Stored/Signed: '{Fingerprint}'; Expected: '{expected}'");
    });
    private readonly GetOnce<(bool Valid, string Message)> _validity = new();
}