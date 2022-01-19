using System;

namespace ToSic.Eav.Configuration.Licenses
{
    /// <summary>
    /// Defines a license - name, guid etc.
    /// </summary>
    public class LicenseDefinition
    {
        public LicenseDefinition(int priority, string name, Guid guid)
        {
            Priority = priority;
            Name = name;
            Guid = guid;
        }

        public readonly int Priority;

        public readonly string Name;
        public readonly Guid Guid;
        public bool AutoEnable = false;
    }
}
