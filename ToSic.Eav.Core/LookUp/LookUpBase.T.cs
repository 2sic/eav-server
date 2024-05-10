using ToSic.Lib.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.LookUp;

[PublicApi]
public abstract class LookUpIn<T>(T data, string name = "source without name", string description = default) : LookUpBase(name, description: description), IWrapper<T>
{
    protected T Data { get; private set; } = data;

    protected void SetData(T newData) => Data = newData;

    public T GetContents() => Data;
}