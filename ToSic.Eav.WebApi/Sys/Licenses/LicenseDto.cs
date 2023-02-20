using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration;

namespace ToSic.Eav.WebApi.Sys.Licenses
{
    public class LicenseDto
    {
        public string Name { get; set; }
        public int Priority { get; set; }
        public Guid Guid { get; set; }
        public string Description { get; set; }

        public bool AutoEnable { get; set; }
        public bool IsEnabled { get; set; }

        public List<FeatureState> Features { get; set; }
    }
}
