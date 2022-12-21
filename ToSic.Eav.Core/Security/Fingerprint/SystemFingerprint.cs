/*
 * Copyright 2022 by 2sic internet solutions in Switzerland - www.2sic.com
 *
 * This file and the code IS COPYRIGHTED.
 * 1. You may not change it.
 * 2. You may not copy the code to reuse in another way.
 *
 * Copying this or creating a similar service, 
 * especially when used to circumvent licensing features in EAV and 2sxc
 * is a copyright infringement.
 *
 * Please remember that 2sic has sponsored more than 10 years of work,
 * and paid more than 1 Million USD in wages for its development.
 * So asking for support to finance advanced features is not asking for much. 
 *
 */

using System;
using System.Reflection;
using ToSic.Eav.Configuration;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;
using ToSic.Eav.Security.Encryption;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Security.Fingerprint
{
    /// <summary>
    /// Class responsible for generating the fingerprint
    /// </summary>
    public sealed class SystemFingerprint: ServiceBase, IFingerprint
    {
        /// <summary>
        /// Constructor - gets Lazy PlatformInfo, because it's only used once for initial generation
        /// </summary>
        public SystemFingerprint(LazySvc<IPlatformInfo> platform, LazySvc<IDbConfiguration> dbConfigLazy): base($"{LogNames.Eav}SysFpr")
        {
            ConnectServices(
                _platform = platform,
                _dbConfig = dbConfigLazy
            );
        }

        private readonly LazySvc<IPlatformInfo> _platform;
        private readonly LazySvc<IDbConfiguration> _dbConfig;

        public string GetFingerprint()
        {
            if (_fingerprintCache != null) return _fingerprintCache;

            var platform = _platform.Value;
            var nameId = platform.Name.ToLowerInvariant();          // usually "dnn" or "oqt"
            var systemGuid = platform.Identity.ToLowerInvariant();  // unique id of an installation
            var sysVersion = platform.Version;                            // Major version, fingerprint should change with each
            var dbConnection = GetDbName().ToLowerInvariant();      // Database name
            var versionEav = Assembly.GetExecutingAssembly().GetName().Version;

            fingerprintKey = $"guid={systemGuid}&platform={nameId}&sys={sysVersion.Major}&eav={versionEav.Major}&db={dbConnection}";
            return _fingerprintCache = Sha256.Hash(fingerprintKey);
        }
        private static string _fingerprintCache;

        /// <summary>
        /// Remember the key for debugging purposes to compare what was used to generate the fingerprint
        /// </summary>
        private static string fingerprintKey;

        private string GetDbName()
        {
            var dbConnection = _dbConfig.Value.ConnectionString;
            const string key = "initial catalog=";
            var dbName = dbConnection.Between(key, ";", true) ?? dbConnection;
            return dbName;
        }

        internal static void ResetForTest() => _fingerprintCache = null;
    }
}
