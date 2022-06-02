namespace ToSic.Eav.DI
{

    public interface IGenerator<out T>
    {
        T New { get; }
    }
}