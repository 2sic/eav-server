using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.StateHelpers
{
    public class StateEntities: StateHelperBase
    {
        private readonly bool _showDrafts;
        public StateEntities(AppState appState, bool showDrafts, ILog parentLog, string logName) : base(appState, parentLog, logName)
        {
            _showDrafts = showDrafts;
        }

        public IImmutableList<IEntity> List => _showDrafts ? AppState.List : AppState.ListPublished.List;


    }
}
