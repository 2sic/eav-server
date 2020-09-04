using System;
using ToSic.Eav.Configuration;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Run;

namespace ToSic.Eav.Core.Tests.Mocks
{
    public class FingerprintProvider: IFingerprint
    {
        public string GetSystemFingerprint() => Guid.NewGuid().ToString();
    }
}
