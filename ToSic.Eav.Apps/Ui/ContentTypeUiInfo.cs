using System.Collections.Generic;

namespace ToSic.Eav.Apps.Ui
{
    public struct ContentTypeUiInfo
    {
        public string Name;
        public string StaticName;
        public bool IsHidden;
        public IDictionary<string, object> Properties;
        public string Thumbnail;
        public bool IsDefault; // new, v13
    }
}
