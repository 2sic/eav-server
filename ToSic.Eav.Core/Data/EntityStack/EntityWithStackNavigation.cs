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
        public EntityWithStackNavigation(IEntity baseEntity, IPropertyStackLookup parent, string field, int index) : base(baseEntity)
        {
            PropertyStackNavigator = new PropertyStackNavigator(baseEntity, parent, field, index);
        }

        
        internal readonly PropertyStackNavigator PropertyStackNavigator;

        public override PropertyRequest FindPropertyInternal(string field, string[] languages, ILog parentLogOrNull)
        {
            var logOrNull = parentLogOrNull.SubLogOrNull(LogNames.Eav + ".EntNav");
            var wrapLog = logOrNull == null 
                ? logOrNull.SafeCall<PropertyRequest>() 
                : logOrNull.SafeCall<PropertyRequest>($"EntityId: {Entity?.EntityId}, Title: {Entity?.GetBestTitle()}, {nameof(field)}: {field}");
            var result = PropertyStackNavigator.PropertyInStack(field, languages, 0, true, logOrNull);

            return wrapLog(result?.Result != null ? "found" : null, result);
        }
    }
}
