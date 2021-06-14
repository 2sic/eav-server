using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// This is a special IEntity-wrapper which will return Stack-Navigation
    /// </summary>
    [PrivateApi]
    public class EntityWithStackNavigation: EntityDecorator
    {
        public EntityWithStackNavigation(IEntity baseEntity, IPropertyStackLookup parent, string field, int index) : base(baseEntity)
        {
            PropertyStackNavigator = new PropertyStackNavigator(baseEntity, parent, field, index);
        }

        
        internal readonly PropertyStackNavigator PropertyStackNavigator;

        public override PropertyRequest FindPropertyInternal(string fieldName, string[] languages) 
            => PropertyStackNavigator.PropertyInStack(fieldName, languages, 0, true);
    }
}
