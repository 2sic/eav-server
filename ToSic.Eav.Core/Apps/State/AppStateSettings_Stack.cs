using System.Collections.Generic;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using static ToSic.Eav.Configuration.ConfigurationStack;

namespace ToSic.Eav.Apps
{
    [PrivateApi]
    public partial class AppStateSettings
    {
        public List<KeyValuePair<string, IPropertyLookup>> GetStack(bool forSettings, IEntity viewPart)
        {
            // View level - always add, no matter if null
            var sources = new List<KeyValuePair<string, IPropertyLookup>>
            {
                new KeyValuePair<string, IPropertyLookup>(PartView, viewPart)
            };

            // All in the App and below
            sources.AddRange(Get(forSettings ? AppThingsToStack.Settings : AppThingsToStack.Resources).FullStack());
            return sources;
        }
    }
}
