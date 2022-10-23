using ToSic.Lib.Documentation;

namespace ToSic.Eav.LookUp
{
    [PublicApi_Stable_ForUseInYourCode]
    public abstract class LookUpIn<T>: LookUpBase
    {
        public T Data { get; protected set; }

        protected LookUpIn(T data, string name = "source without name")
        {
            Data = data;
            Name = name;
        }
    }
}
