using System;
using ToSic.Eav.Configuration;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Core.Tests.Mocks
{
    public class FingerprintProvider: IFingerprintProvider
    {
        public string GetSystemFingerprint() => Guid.NewGuid().ToString();
    }
}
