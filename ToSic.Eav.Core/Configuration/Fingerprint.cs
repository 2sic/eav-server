using System;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Configuration
{
    public static class Fingerprint
    {
        public static string System => _system ?? (_system = LoadSystemFingerprint());

        private static string _system;

        private static string LoadSystemFingerprint()
        {
            try
            {
                var provider = Factory.Resolve<IFingerprintProvider>();
                return provider.GetSystemFingerprint();
            }
            catch
            {
                return "error-generating-fingerprint-use-random:" + new Random().Next();
            }
        }
    }
}
