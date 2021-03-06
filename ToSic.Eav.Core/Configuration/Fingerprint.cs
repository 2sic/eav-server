﻿using System;
using ToSic.Eav.Documentation;
using ToSic.Eav.Run;

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// System to provide unique fingerprints (IDs) for certain parts of the EAV
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public static class Fingerprint
    {
        /// <summary>
        /// The system fingerprint of the current execution environment.
        /// Used for signing data that should be tamper-proof
        /// </summary>
        public static string System => _system ?? (_system = LoadSystemFingerprint());

        private static string _system;

        private static string LoadSystemFingerprint()
        {
            try
            {
                var provider = Factory.StaticBuild<IFingerprint>();
                return provider.GetSystemFingerprint();
            }
            catch
            {
                return "error-generating-fingerprint-use-random:" + new Random().Next();
            }
        }
    }
}
