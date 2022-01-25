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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;
using ToSic.Eav.Security.Encryption;
using ToSic.Eav.Security.Fingerprint;

namespace ToSic.Eav.Configuration.Licenses
{
    /// <summary>
    /// Will check the loaded licenses and prepare validity information for use during system runtime
    /// </summary>
    internal sealed class LicenseLoader: LoaderBase
    {
        /// <summary>
        /// Constructor - not meant for DI
        /// </summary>
        internal LicenseLoader(/*SystemFingerprint fingerprint,*/ LogHistory logHistory, ILog parentLog)
            : base(/*fingerprint,*/ logHistory, parentLog, LogNames.Eav + "LicLdr", "Load Licenses")
        {
        }

        private string _fingerprint;

        public LicenseLoader Init(string fingerprint)
        {
            _fingerprint = fingerprint;
            return this;
        }

        /// <summary>
        /// Pre-Load enabled / disabled global features
        /// </summary>
        [PrivateApi]
        internal void LoadLicenses(AppState presetApp)
        {
            var wrapLog = Log.Call();
            try
            {
                var licenseEntities = presetApp.List.OfType(LicenseConstants.TypeName).ToList();
                Log.Add($"Found {licenseEntities.Count} license entities");
                var autoEnabled = AutoEnabledLicenses();
                var licenses = licenseEntities.SelectMany(LicensesInOneEntity).ToList();
                LicenseService.Update(autoEnabled.Union(licenses).ToList());
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
            //var myFingerprint = Fingerprint;
            var fps = infoRaw.Fingerprints.SplitNewLine().TrimmedAndWithoutEmpty();
            var validFp = fps.Any(fp => _fingerprint.Equals(fp));
            Log.Add($"Fingerprint: {validFp}");

            var validVersion = int.TryParse(infoRaw.Version, out var licVersion) &&
                               SystemInformation.Version.Major == licVersion;
            Log.Add($"Version: {validVersion}");

            var dateOk = DateTime.TryParse(infoRaw.Expires, out var expires);
            if(!dateOk)
                expires = DateTime.MinValue;

            var validDate = dateOk && DateTime.Now.CompareTo(expires) <= 0;
            Log.Add($"Expired: {validDate}");

            var licenses = infoRaw.Licenses.SplitNewLine().TrimmedAndWithoutEmpty() ?? Array.Empty<string>();
            Log.Add($"Licenses: {licenses.Length}");

            var licenseStates = licenses.Select(l => new LicenseState
                {
                    Title = infoRaw.Name,
                    License = LicenseCatalog.Find(l),
                    EntityGuid = infoRaw.Guid,
                    LicenseKey = infoRaw.Key,
                    Expiration = expires,
                    ValidExpired = validDate,
                    ValidFingerprint = validFp,
                    ValidSignature = validSig,
                    ValidVersion = validVersion,
                })
                .ToList();

            return wrapLog(licenseStates.Count.ToString(), licenseStates);
        }

        private List<LicenseState> AutoEnabledLicenses()
        {
            var licenseStates = LicenseCatalog.Licenses.Where(l => l.AutoEnable).Select(l => new LicenseState
                {
                    Title = l.Name,
                    License = l,
                    EntityGuid = Guid.Empty,
                    LicenseKey = "always enabled",
                    Expiration = LicenseCatalog.UnlimitedExpiry,
                    ValidExpired = true,
                    ValidFingerprint = true,
                    ValidSignature = true,
                    ValidVersion = true,
                })
                .ToList();
            return licenseStates;
        }

    }
}
