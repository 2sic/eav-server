using ToSic.Lib.Data;

namespace ToSic.Lib.LookUp.Sources;

[PublicApi]
public abstract class LookUpIn<T>(T data, string name = "source without name", string? description = default)
    : LookUpBase(name, description: description), IWrapper<T>
{
    protected T Data { get; private set; } = data;

    protected void SetData(T newData) => Data = newData;

    public T GetContents() => Data;
}