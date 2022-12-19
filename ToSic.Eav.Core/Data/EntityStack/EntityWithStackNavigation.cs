using ToSic.Eav.Data.PropertyLookup;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// This is a special IEntity-wrapper which will return Stack-Navigation
    /// </summary>
    [PrivateApi]
    public class EntityWithStackNavigation: EntityWrapper, IPropertyStackLookup
    {
        public EntityWithStackNavigation(IEntity entity, StackAddress stackAddress) : base(entity) 
            => Navigator = new PropertyStackNavigator(entity, stackAddress);
        internal readonly PropertyStackNavigator Navigator;

        public override PropReqResult FindPropertyInternal(PropReqSpecs specs, PropertyLookupPath path) =>
            PropertyLookupWithStackNavigation.FindPropertyInternalOfStackWrapper(this, specs, path,
                LogNames.Eav + ".EntNav", $"EntityId: {Entity?.EntityId}, Title: {Entity?.GetBestTitle()}");

        public PropReqResult GetNextInStack(PropReqSpecs specs, int startAtSource, PropertyLookupPath path) 
            => Navigator.GetNextInStack(specs, startAtSource, path);
    }
}
