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

using ToSic.Eav.Security.Encryption;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using ToSic.Sys.Capabilities.Platform;
using ToSic.Sys.Configuration;
using ToSic.Sys.Utils;

namespace ToSic.Sys.Capabilities.Fingerprints;

/// <summary>
/// Class responsible for generating the fingerprint
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class SystemFingerprint(LazySvc<IPlatformInfo> platform, LazySvc<IGlobalConfiguration> globalConfig)
    : ServiceBase("Sys.SysFpr", connect: [platform]), IFingerprint
{
    public string GetFingerprint()
    {
        if (_fingerprintCache != null)
            return _fingerprintCache;

        var platform1 = platform.Value;
        var nameId = platform1.Name.ToLowerInvariant();          // usually "dnn" or "oqt"
        var systemGuid = platform1.Identity.ToLowerInvariant();  // unique id of an installation
        var sysVersion = platform1.Version;                            // Major version, fingerprint should change with each
        var dbConnection = GetDbName().ToLowerInvariant();      // Database name
        var versionEav = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
            ?? throw new NullReferenceException("Version is missing");

        _fingerprintKey = $"guid={systemGuid}&platform={nameId}&sys={sysVersion.Major}&eav={versionEav.Major}&db={dbConnection}";
        return _fingerprintCache = Sha256.Hash(_fingerprintKey);
    }
    private static string? _fingerprintCache;

    /// <summary>
    /// Remember the key for debugging purposes to compare what was used to generate the fingerprint
    /// </summary>
    private static string? _fingerprintKey;

    private string GetDbName()
    {
        // ReSharper disable once ExplicitCallerInfoArgument
        var dbConnection = globalConfig.Value.GetThis(key: "ConnectionString") ?? throw new NullReferenceException("Needs ConnectionString");
        const string key = "initial catalog=";
        var dbName = dbConnection.Between(key, ";", true) ?? dbConnection;
        return dbName;
    }

    internal static void ResetForTest() => _fingerprintCache = null;

    public List<EnterpriseFingerprint> EnterpriseFingerprints => _enterpriseFingerprintsStatic;
    private static List<EnterpriseFingerprint> _enterpriseFingerprintsStatic = [];

    public void LoadEnterpriseFingerprints(List<EnterpriseFingerprint> enterpriseFingerprints) 
        => _enterpriseFingerprintsStatic = enterpriseFingerprints;
}