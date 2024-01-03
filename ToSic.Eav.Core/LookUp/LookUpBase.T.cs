using ToSic.Lib.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.LookUp;

[PublicApi]
public abstract class LookUpIn<T>: LookUpBase, IWrapper<T>
{
    protected T Data { get; private set; }

    protected void SetData(T data) => Data = data;

    public T GetContents() => Data;

    protected LookUpIn(T data, string name = "source without name")
    {
        Data = data;
        Name = name;
    }
}