namespace ToSic.Eav.Data.Process
{
    public interface IEntityPair<out TPartner>: ICanBeEntity
    {
        TPartner Partner { get; }
    }
}
