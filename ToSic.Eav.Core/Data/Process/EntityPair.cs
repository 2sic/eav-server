namespace ToSic.Eav.Data.Process
{
    /// <summary>
    /// Special object to carry an IEntity and another object which belong together.
    ///
    /// This is mainly used in scenarios where you create new Entities and must keep the original tied to the Entity
    /// because you may still need the original later on for further processing. 
    /// </summary>
    /// <remarks>
    /// Added in 15.04
    /// </remarks>
    /// <typeparam name="TPartner"></typeparam>
    public class EntityPair<TPartner>: ICanBeEntity
    {
        public EntityPair(TPartner partner, IEntity entity)
        {
            Partner = partner;
            Entity = entity;
        }

        public TPartner Partner { get; }

        public IEntity Entity { get; }
    }
}
