using System;
using System.Collections.Generic;

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

        public bool IsGlobal { get; }

        public string Errors { get; }

        public DataSourceDto(DataSourceInfo dsInfo, ICollection<string> outNames)
        {
            var dsAttribute = dsInfo.VisualQuery;
            Name = dsInfo.Type.Name; // will override further down if dsInfo is provided
            Identifier = dsInfo.Name;
            if (dsAttribute == null) return;
            UiHint = dsAttribute.UiHint;
            PrimaryType = dsAttribute.Type.ToString();
            Icon = dsAttribute.Icon; // ?.Replace("_", "-"); // wip: rename "xxx_yyy" icons to "xxx-yyy" - must switch to base64 soon
            HelpLink = dsAttribute.HelpLink;
            In = dsAttribute.In ?? Array.Empty<string>();
            DynamicOut = dsAttribute.DynamicOut;
            DynamicIn = dsAttribute.DynamicIn;
            EnableConfig = dsAttribute.EnableConfig;
            ContentType = dsAttribute.ConfigurationType;
            if (!string.IsNullOrEmpty(dsAttribute.NiceName))
                Name = dsAttribute.NiceName;
            Difficulty = (int)dsAttribute.Audience;
            Audience = (int)dsAttribute.Audience;
            IsGlobal = dsInfo.IsGlobal;

            TypeNameForUi = dsInfo.Type.FullName;
            Out = outNames;
            Errors = dsInfo.ErrorOrNull?.Message;

            // WIP try to deprecate
            PartAssemblyAndType = dsInfo.Name;
        }
    }
}