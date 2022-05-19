using ToSic.Eav.Data.PropertyLookup;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// This is a special IEntity-wrapper which will return Stack-Navigation
    /// </summary>
    [PrivateApi]
    public class EntityWithStackNavigation: EntityWrapper
    {
        public EntityWithStackNavigation(IEntity entity, IPropertyStackLookup parent, string field, int index, int depth) : base(entity)
        {
            PropertyStackNavigator = new PropertyStackNavigator(entity, parent, field, index, depth);
        }

        
        internal readonly PropertyStackNavigator PropertyStackNavigator;

        public override PropertyRequest FindPropertyInternal(string field, string[] languages, ILog parentLogOrNull, PropertyLookupPath path)
        {
            var logOrNull = parentLogOrNull.SubLogOrNull(LogNames.Eav + ".EntNav");
            var wrapLog = logOrNull.Call2<PropertyRequest>(
                $"EntityId: {Entity?.EntityId}, Title: {Entity?.GetBestTitle()}, {nameof(field)}: {field}");
            var result = PropertyStackNavigator.PropertyInStack(field, languages, 0, true, logOrNull, path);

            return wrapLog.Return(result, result?.Result != null ? "found" : null);
        }
    }
}
