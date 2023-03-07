namespace ToSic.Eav.Data.New
{
    public class NewEntitySet<TNew>: ICanBeEntity
    {
        public NewEntitySet(TNew original, IEntity entity)
        {
            Original = original;
            Entity = entity;
        }

        public TNew Original { get; }

        public IEntity Entity { get; }
    }
}
