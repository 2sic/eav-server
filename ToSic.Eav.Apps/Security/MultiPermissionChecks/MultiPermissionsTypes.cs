﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Context;
using ToSic.Lib.Logging;
using ToSic.Eav.Security;


namespace ToSic.Eav.Apps.Security
{
    public class MultiPermissionsTypes: MultiPermissionsApp
    {
        private const string LogName = "Sec.MPTyps";
        protected IEnumerable<string> ContentTypes;

        public MultiPermissionsTypes(Dependencies dependencies, Lazy<IAppStates> appStates): base(dependencies)
        {
            _appStates = appStates;
        }
        private readonly Lazy<IAppStates> _appStates;

        // Note: AppState must be public, as we have some extension methods that need it
        public AppState AppState => _appState ?? (_appState = App as AppState ?? _appStates.Value.Get(App));
        private AppState _appState;

        public MultiPermissionsTypes Init(IContextOfSite context, IAppIdentity app, string contentType, ILog parentLog)
        {
            Init(context, app, parentLog, LogName);
            return InitTypesAfterInit(new[] {contentType});
        }

        //public MultiPermissionsTypes Init(IContextOfSite context, IAppIdentity app, IEnumerable<string> contentTypes, ILog parentLog)
        //{ 
        //   Init(context, app, parentLog, LogName);
        //   return InitTypes(contentTypes);
        //}

        /// <summary>
        /// This step is separate, because extension methods need it _after_ Init
        /// </summary>
        /// <param name="contentTypes"></param>
        /// <returns></returns>
        public MultiPermissionsTypes InitTypesAfterInit(IEnumerable<string> contentTypes)
        {
            ContentTypes = contentTypes;
            return this;
        }

        //public MultiPermissionsTypes Init(IContextOfSite context, IAppIdentity app, List<ItemIdentifier> items, ILog parentLog)
        //{
        //    Init(context, app, parentLog, LogName);
        //    ContentTypes = ExtractTypeNamesFromItems(items);
        //    return this;
        //}


        protected override Dictionary<string, IPermissionCheck> InitializePermissionChecks()
            => InitPermissionChecksForType(ContentTypes);

        protected Dictionary<string, IPermissionCheck> InitPermissionChecksForType(IEnumerable<string> contentTypes)
            => contentTypes.Distinct().ToDictionary(t => t, BuildTypePermissionChecker);

        //private IEnumerable<string> ExtractTypeNamesFromItems(IEnumerable<ItemIdentifier> items)
        //{
        //    var appData = AppState.List;
        //    // build list of type names
        //    var typeNames = items.Select(item =>
        //        !string.IsNullOrEmpty(item.ContentTypeName) || item.EntityId == 0
        //            ? item.ContentTypeName
        //            : appData.FindRepoId(item.EntityId).Type.NameId);

        //    return typeNames;
        //}


        /// <summary>
        /// Creates a permission checker for an type in this app
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        private IPermissionCheck BuildTypePermissionChecker(string typeName)
        {
            Log.A($"BuildTypePermissionChecker({typeName})");
            // now do relevant security checks
            return BuildPermissionChecker(AppState.GetContentType(typeName));
        }
    }
}
