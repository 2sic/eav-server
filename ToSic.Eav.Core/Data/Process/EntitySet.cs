namespace ToSic.Eav.Data.Process
{
    public class EntitySet<TPartner, TAssistant>: EntityPair<TPartner> // where TPartner : class where TAssistant : class
    {
        public EntitySet(EntityPair<TPartner> original, TAssistant assistant) : this(original.Entity, original.Partner, assistant) { }

        public EntitySet(IEntity entity, TPartner partner, TAssistant assistant) : base(entity, partner)
        {
            Assistant = assistant;
        }

        public TAssistant Assistant { get; }

        public EntitySet<TPartner, TAssistant> Clone(IEntity entity = default, TPartner partner = default, TAssistant assistant = default)//, TPartner partner = default, TAssistant assistant = default)
            => new EntitySet<TPartner, TAssistant>(entity ?? Entity, partner != null ? partner : Partner, assistant != null ? assistant : Assistant);//, partner ?? Partner, assistant ?? Assistant);
    }
}
