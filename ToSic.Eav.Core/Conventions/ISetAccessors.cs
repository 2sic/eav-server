namespace ToSic.Eav.Conventions
{
    public interface ISetAccessors<in TValue>
    {
        void Set(string name, TValue value);
    }
    public interface ISetAccessorsGeneric
    {
        void Set<TValue>(string name, TValue value);
    }
    public interface ISetAccessors<in TValue, out TResult>
    {
        TResult Set(string name, TValue value);
    }
}
