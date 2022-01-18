using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;
using ToSic.Eav.Security.Encryption;

namespace ToSic.Eav.Configuration.Licenses
{
    /// <summary>
    /// Will check the loaded licenses and prepare validity information for use during system runtime
    /// </summary>
    internal sealed class LicenseLoader: HasLog
    {
        /// <summary>
        /// Constructor - not meant for DI
        /// </summary>
        internal LicenseLoader(IAppsCache appsCache, IFingerprint fingerprint, LogHistory logHistory, ILog parentLog) : base(LogNames.Eav + "LicLdr", parentLog, "Load Licenses")
        {
            _appsCache = appsCache;
            _fingerprint = fingerprint;
            logHistory.Add(LogNames.LogHistoryGlobalTypes, Log);
        }
        private readonly IAppsCache _appsCache;
        private readonly IFingerprint _fingerprint;


        /// <summary>
        /// Pre-Load enabled / disabled global features
        /// </summary>
        [PrivateApi]
        public void LoadLicenses()
        {
            var wrapLog = Log.Call();
            try
            {
                var presetApp = _appsCache.Get(null, Constants.PresetIdentity);
                var licenseEntities = presetApp.List.OfType(LicenseConstants.TypeName).ToList();
                Log.Add($"Found {licenseEntities.Count} license entities");
                var licenses = licenseEntities.SelectMany(LicensesInOneEntity).ToList();
                Licenses.All = licenses;
                Licenses.CacheTimestamp = DateTime.Now.Ticks;
                Log.Add($"Found {licenses.Count} licenses");
                wrapLog("ok");
            }
            catch (Exception ex)
            {
                // Just log and ignore
                Log.Exception(ex);
                wrapLog("error");
            }
        }

        private List<LicenseState> LicensesInOneEntity(IEntity entity)
        {
            var wrapLog = Log.Call<List<LicenseState>>();

            if (entity == null) return wrapLog("null", new List<LicenseState>());

            var infoRaw = new LicenseInfoRaw(entity);

            // Check signature valid
            var resultForSignature = infoRaw.GetStandardizedControlString();
            var validSig = false;
            try
            {
                var data = new UnicodeEncoding().GetBytes(resultForSignature);
                validSig = new Sha256().VerifyBase64(FeatureConstants.FeaturesValidationSignature2Sxc930,
                    infoRaw.Signature, data);
            }
            catch (Exception ex)
            {
                // Just log, and ignore
                Log.Exception(ex);
            }
            Log.Add($"Signature: {validSig}");

            // Check fingerprints
            var myFingerprint = _fingerprint.GetSystemFingerprint();
            var fps = infoRaw.Fingerprints.SplitNewLine().TrimmedAndWithoutEmpty();
            var validFp = fps.Any(fp => myFingerprint.Equals(fp));
            Log.Add($"Fingerprint: {validFp}");

            var validVersion = int.TryParse(infoRaw.Version, out var licVersion) &&
                               SystemInformation.Version.Major == licVersion;
            Log.Add($"Version: {validVersion}");

            var validDate = DateTime.TryParse(infoRaw.Expires, out var expires) && DateTime.Now.CompareTo(expires) <= 0;
            Log.Add($"Expired: {validDate}");

            var licenses = infoRaw.Licenses.SplitNewLine().TrimmedAndWithoutEmpty() ?? Array.Empty<string>();
            Log.Add($"Licenses: {licenses.Length}");

            var licenseStates = licenses.Select(l => new LicenseState
                {
                    License = LicenseTypes.Find(l),
                    EntityGuid = infoRaw.Guid,
                    LicenseKey = infoRaw.Key,
                    ValidExpired = validDate,
                    ValidFingerprint = validFp,
                    ValidSignature = validSig,
                    ValidVersion = validVersion,
                })
                .ToList();

            return wrapLog(licenseStates.Count.ToString(), licenseStates);
        }
    }
}
