using System;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Shared;
using ToSic.Eav.Documentation;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps
{
    [PrivateApi("WIP v13")]
    public static class AppStateMetadataTargetExtensions
    {
        // 2021-11-22 2dm WIP - not used yet
        // Idea was to be able to get Metadata Targets, but I'm not sure if it's useful at all
        public static string FindTargetTitle(this AppState appState, int targetType, string key)
        {

            if (!Enum.IsDefined(typeof(TargetTypes), targetType)) return null;
            var tType = (TargetTypes)targetType;
            
            switch (tType)
            {
                case TargetTypes.None: 
                    return null;
                case TargetTypes.Attribute:
                    if (!int.TryParse(key, out var keyInt)) return null;
                    var attr = appState.FindAttribute(keyInt);
                    return attr?.Item1?.Metadata?.Target.Title + "/" + attr?.Item2?.Metadata?.Target.Title;
                case TargetTypes.App:
                    return appState.Metadata?.Target.Title;
                case TargetTypes.Entity:
                    if (!Guid.TryParse(key, out var guidKey)) return null;
                    return appState.List.One(guidKey)?.Metadata?.Target.Title;
                case TargetTypes.ContentType:
                    return appState.GetContentType(key)?.Metadata?.Target.Title;
                case TargetTypes.Zone:
                    return null;
                case TargetTypes.CmsItem:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static Tuple<IContentType, IContentTypeAttribute> FindAttribute(this AppState appState, string idString) 
            => !int.TryParse(idString, out var keyInt) ? null : appState.FindAttribute(keyInt);

        public static Tuple<IContentType, IContentTypeAttribute> FindAttribute(this AppState appState, int id)
        {
            var allLocalCts = appState.ContentTypes
                .Where(ct => !ct.HasAncestor());
            var attr = allLocalCts
                .SelectMany(ct => ct.Attributes.Select(at => new Tuple<IContentType, IContentTypeAttribute>(ct, at)))
                .FirstOrDefault(set => set.Item2.AttributeId == id);
            return attr;
        }
    }
}
