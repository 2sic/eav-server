using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    [PrivateApi]
    public partial class AppStateSettings
    {

        public AppState Parent { get; }

        internal AppStateSettings(AppState parent)
        {
            Parent = parent;

            Stacks = ConfigurationConstants.AppThingsArray
                .Select(at => new AppStateStackCache(parent, at))
                .ToArray();

            StackCache = ConfigurationConstants.AppThings
                .ToDictionary(
                    at => at.Key,
                    at => new AppStateStackCache(parent, at.Value)
                )
                .ToImmutableDictionary();
        }

        public AppStateStackCache[] Stacks;
        public AppStateStackCache Get(AppThingsToStack target) => Stacks.First(s => s.Target.Target == target);

        public IImmutableDictionary<AppThingsToStack, AppStateStackCache> StackCache { get; }

    }
}
