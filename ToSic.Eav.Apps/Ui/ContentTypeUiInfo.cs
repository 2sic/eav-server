﻿using System.Collections.Generic;

namespace ToSic.Eav.Apps.Ui
{
    public struct ContentTypeUiInfo
    {
        public string Name;
        public string StaticName;
        public bool IsHidden;
        public Dictionary<string, object> Metadata;
        public string Thumbnail;
    }
}
