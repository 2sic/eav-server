using System;

namespace ToSic.Eav.Configuration.Licenses
{
    public class LicenseType
    {
        public LicenseType(string name, Guid guid)
        {
            Name = name;
            Guid = guid;
        }

        public string Name;
        public Guid Guid;
    }
}
