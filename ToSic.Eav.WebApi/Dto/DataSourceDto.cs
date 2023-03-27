﻿using System;
using System.Collections.Generic;
using ToSic.Eav.DataSource.VisualQuery;

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

        private static readonly string[] StreamNamesIfError = { DataSourceConstants.StreamDefaultName };

        public DataSourceDto(DataSourceInfo dsInfo, ICollection<string> outNameList)
        {
            var noError = dsInfo.ErrorOrNull == null;
            var dsAttribute = dsInfo.VisualQuery;
            Name = noError ? dsInfo.Type.Name : dsInfo.Name; // will override further down if dsInfo is provided
            Identifier = dsInfo.Name;
            if (dsAttribute == null) return;
            UiHint = dsAttribute.UiHint;
            PrimaryType = dsAttribute.Type.ToString();
            Icon = dsAttribute.Icon; // ?.Replace("_", "-"); // wip: rename "xxx_yyy" icons to "xxx-yyy" - must switch to base64 soon
            HelpLink = dsAttribute.HelpLink;
            In = noError ? dsAttribute.In ?? Array.Empty<string>() : StreamNamesIfError;
            DynamicOut = dsAttribute.DynamicOut;
            DynamicIn = dsAttribute.DynamicIn;
            EnableConfig = dsAttribute.EnableConfig;
            ContentType = dsAttribute.ConfigurationType;
            if (!string.IsNullOrEmpty(dsAttribute.NiceName))
                Name = dsAttribute.NiceName;
            Difficulty = (int)dsAttribute.Audience;
            Audience = (int)dsAttribute.Audience;
            IsGlobal = dsInfo.IsGlobal;

            // If we have a substituted error-DS, give it the inner name so connections work
            TypeNameForUi = noError ? dsInfo.Type.FullName : dsInfo.Name;
            Out = noError ? outNameList : StreamNamesIfError;
            Errors = dsInfo.ErrorOrNull?.Message;

            // WIP try to deprecate
            PartAssemblyAndType = dsInfo.Name;
        }
    }
}