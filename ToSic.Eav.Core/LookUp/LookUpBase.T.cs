using ToSic.Eav.Documentation;

namespace ToSic.Eav.LookUp
{
    [PublicApi]
    public abstract class LookUpBase<T>: LookUpBase
    {
        public T Data { get; protected set; }

        protected LookUpBase(T data, string name = "source without name")
        {
            Data = data;
            Name = name;
        }
    }
}
