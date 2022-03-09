using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Security;
using ToSic.Eav.Context;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi.Security
{
    /// <summary>
    /// Extension Methods to Init with slightly different parameters
    /// </summary>
    internal static class MultiPermissionTypeExtensions
    {
        public static MultiPermissionsTypes Init(this MultiPermissionsTypes parent, IContextOfSite context, IAppIdentity app, List<ItemIdentifier> items, ILog parentLog)
        {
            parent.Init(context, app, parentLog);
            var contentTypes = ExtractTypeNamesFromItems(parent, items);
            return parent.InitTypesAfterInit(contentTypes);
        }

        /// <summary>
        /// Important: this can only run after init, because AppState isn't available before
        /// </summary>
        private static IEnumerable<string> ExtractTypeNamesFromItems(MultiPermissionsTypes parent, IEnumerable<ItemIdentifier> items)
        {
            var appData = parent.AppState.List;
            // build list of type names
            var typeNames = items.Select(item =>
                !string.IsNullOrEmpty(item.ContentTypeName) || item.EntityId == 0
                    ? item.ContentTypeName
                    : appData.FindRepoId(item.EntityId).Type.NameId);

            return typeNames;
        }

    }
}
