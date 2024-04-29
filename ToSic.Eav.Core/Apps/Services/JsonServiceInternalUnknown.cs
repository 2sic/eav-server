using System;
using ToSic.Eav.Internal.Unknown;

namespace ToSic.Eav.Apps.Services;

public class JsonServiceInternalUnknown : IJsonServiceInternal
{
    public JsonServiceInternalUnknown(WarnUseOfUnknown<JsonServiceInternalUnknown> _) { }
    public string ToJson(object item)
    {
        throw new NotImplementedException();
    }

    public string ToJson(object item, int indentation)
    {
        throw new NotImplementedException();
    }

    public T To<T>(string json)
    {
        throw new NotImplementedException();
    }

    public object ToObject(string json)
    {
        throw new NotImplementedException();
    }
}