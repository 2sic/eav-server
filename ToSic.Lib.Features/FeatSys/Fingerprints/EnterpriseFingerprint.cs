using ToSic.Eav.Security.Encryption;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.SysData;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EnterpriseFingerprint
{
    public int Id { get; init; }
    public Guid Guid { get; init; }
    public string Title { get; init; }
    public string Fingerprint { get; init; }
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