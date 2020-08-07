using System;
using ToSic.Eav.Configuration;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Run;

namespace ToSic.Eav.Core.Tests.Mocks
{
    public class FingerprintProvider: IFingerprintProvider
    {
        public string GetSystemFingerprint() => Guid.NewGuid().ToString();
    }
}
