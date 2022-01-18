using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ToSic.Eav.Data;

namespace ToSic.Eav.Configuration.Licenses
{
    public class LicenseInfoRaw
    {
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public string Key { get; set; }
        
        public string Licenses { get; set; }

        public string Fingerprints { get; set; }


        public string Version { get; set; }

        public string Expires { get; set; }

        public string Signature { get; set; }

        /// <summary>
        /// Empty constructor so it can also be used by code on 2sxc.org to create licenses
        /// </summary>
        public LicenseInfoRaw() { }

        public LicenseInfoRaw(IEntity entity)
        {
            Guid = entity.EntityGuid;
            Name = entity.Value<string>(LicenseConstants.FieldTitle);
            Key = entity.Value<string>(LicenseConstants.FieldKey);
            Licenses = entity.Value<string>(LicenseConstants.FieldLicenses);
            Fingerprints = entity.Value<string>(LicenseConstants.FieldFingerprint);

            var restrictionsJson = entity.Value<string>(LicenseConstants.FieldRestrictions) ?? "";
            var restrictions = JsonConvert.DeserializeObject<LicenseRestrictions>(restrictionsJson);
            Expires = restrictions.Expires;
            Version = restrictions.Version;

            Signature = entity.Value<string>(LicenseConstants.FieldSignature);
        }

        public string GetStandardizedControlString()
        {
            var licenseString = ""
                                + "key: " + Key
                                + ";licenses:" + Licenses // 2. Add all licenses
                                + ";fingerprints:" + Fingerprints // 3. Add all fingerprints
                                + ";expires:" + Expires
                                + ";version:" + Version
                                + ";salt:" + Guid;

            var licNoNl = Regex.Replace(licenseString, @"\r\n?|\n", ",");
            var licNoSpaces = Regex.Replace(licNoNl, @"\s+", "");
            return licNoSpaces;
        }
    }
}
