using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;
using ToSic.Eav.Security;
using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi.Security
{
    public class MultiPermissionsTypes: MultiPermissionsApp
    {
        private const string LogName = "Sec.MPTyps";
        protected IEnumerable<string> ContentTypes;

        public MultiPermissionsTypes(IZoneMapper zoneMapper, IAppStates appStates): base(zoneMapper)
        {
            _appStates = appStates;
        }
        private readonly IAppStates _appStates;

        public MultiPermissionsTypes Init(IContextOfSite context, IAppIdentity app, string contentType, ILog parentLog)
        {
            Init(context, app, new[] {contentType}, parentLog);
            return this;
        }

        public MultiPermissionsTypes Init(IContextOfSite context, IAppIdentity app, IEnumerable<string> contentTypes, ILog parentLog)
        { 
           Init(context, app, parentLog, LogName);
           ContentTypes = contentTypes;
           return this;
        }

        public MultiPermissionsTypes Init(IContextOfSite context, IAppIdentity app, List<ItemIdentifier> items, ILog parentLog)
        {
            Init(context, app, parentLog, LogName);
            ContentTypes = ExtractTypeNamesFromItems(items);
            return this;
        }


        protected override Dictionary<string, IPermissionCheck> InitializePermissionChecks()
            => InitPermissionChecksForType(ContentTypes);

        protected Dictionary<string, IPermissionCheck> InitPermissionChecksForType(IEnumerable<string> contentTypes)
            => contentTypes.Distinct().ToDictionary(t => t, BuildTypePermissionChecker);

        private IEnumerable<string> ExtractTypeNamesFromItems(IEnumerable<ItemIdentifier> items)
        {
            var appData = AppState.List;
            // build list of type names
            var typeNames = items.Select(item =>
                !string.IsNullOrEmpty(item.ContentTypeName) || item.EntityId == 0
                    ? item.ContentTypeName
                    : appData.FindRepoId(item.EntityId).Type.NameId);

            return typeNames;
        }

        protected AppState AppState => _appState ?? (_appState = _appStates.Get(App));
        private AppState _appState;

        /// <summary>
        /// Creates a permission checker for an type in this app
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private IPermissionCheck BuildTypePermissionChecker(string typeName)
        {
            Log.Add($"BuildTypePermissionChecker({typeName})");
            // now do relevant security checks
            return BuildPermissionChecker(AppState.GetContentType(typeName));
        }
    }
}
