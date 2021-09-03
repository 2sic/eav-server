using System.Collections.Generic;
using ToSic.Eav.DataSources.Queries;

namespace ToSic.Eav.DataSources.Catalog
{
    public class DataSourceDto
    {
        public string PartAssemblyAndType;
        public ICollection<string> In;
        public ICollection<string> Out;
        public string ContentType;
        public string PrimaryType;
        public string Icon;
        public bool DynamicOut;
        public bool DynamicIn;
        public string HelpLink;
        public bool EnableConfig;
        public string Name;
        public string UiHint;
        public int Difficulty;

        public DataSourceDto(string fallbackName, VisualQueryAttribute dsInfo)
        {
            Name = fallbackName; // will override further down if dsInfo is provided
            if (dsInfo == null) return;
            UiHint = dsInfo.UiHint;
            PrimaryType = dsInfo.Type.ToString();
            Icon = dsInfo.Icon;
            HelpLink = dsInfo.HelpLink;
            In = dsInfo.In;
            DynamicOut = dsInfo.DynamicOut;
            DynamicIn = dsInfo.DynamicIn;
            EnableConfig = dsInfo.EnableConfig;
            ContentType = dsInfo.ExpectsDataOfType;
            if (!string.IsNullOrEmpty(dsInfo.NiceName))
                Name = dsInfo.NiceName;
            Difficulty = (int)dsInfo.Difficulty;
        }
    }
}