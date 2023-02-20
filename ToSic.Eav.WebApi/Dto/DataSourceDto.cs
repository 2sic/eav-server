using System.Collections.Generic;
using ToSic.Eav.DataSources.Queries;

namespace ToSic.Eav.DataSources.Catalog
{
    public class DataSourceDto
    {
        public string TypeNameForUi { get; set; }

        // old, try to deprecate and replace with Identifier
        public string PartAssemblyAndType { get; set; }

        public string Identifier { get; set; }

        public ICollection<string> In { get; set; }
        public ICollection<string> Out { get; set; }
        public string ContentType { get; set; }
        public string PrimaryType { get; set; }
        public string Icon { get; set; }
        public bool DynamicOut { get; set; }
        public bool DynamicIn { get; set; }
        public string HelpLink { get; set; }
        public bool EnableConfig { get; set; }
        public string Name { get; set; }
        public string UiHint { get; set; }

        // todo: deprecated, but probably still in use in Visual Query - should be replaced by Audience
        public int Difficulty { get; set; }
        public int Audience { get; set; }

        public DataSourceDto(string fallbackName, VisualQueryAttribute dsInfo)
        {
            Name = fallbackName; // will override further down if dsInfo is provided
            if (dsInfo == null) return;
            UiHint = dsInfo.UiHint;
            PrimaryType = dsInfo.Type.ToString();
            Icon = dsInfo.Icon; // ?.Replace("_", "-"); // wip: rename "xxx_yyy" icons to "xxx-yyy" - must switch to base64 soon
            HelpLink = dsInfo.HelpLink;
            In = dsInfo.In;
            DynamicOut = dsInfo.DynamicOut;
            DynamicIn = dsInfo.DynamicIn;
            EnableConfig = dsInfo.EnableConfig;
            ContentType = dsInfo.ExpectsDataOfType;
            if (!string.IsNullOrEmpty(dsInfo.NiceName))
                Name = dsInfo.NiceName;
            Difficulty = (int)dsInfo.Audience;
            Audience = (int)dsInfo.Audience;
        }
    }
}