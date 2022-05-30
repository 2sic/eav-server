namespace ToSic.Eav.Plumbing
{

    public interface IGenerator<out T>
    {
        T New { get; }
    }
}