namespace ToSic.Lib.DI
{

    public interface IGenerator<out T>
    {
        T New();
    }
}