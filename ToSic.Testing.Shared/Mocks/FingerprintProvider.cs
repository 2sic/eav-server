using System;
using ToSic.Eav.Run;

namespace ToSic.Testing.Shared.Mocks
{
    public class FingerprintProvider: IFingerprint
    {
        public string GetSystemFingerprint() => Guid.NewGuid().ToString();
    }
}
