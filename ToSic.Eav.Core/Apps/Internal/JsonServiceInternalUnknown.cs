using System;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps.Internal;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class JsonServiceInternalUnknown : IJsonServiceInternal
{
    public JsonServiceInternalUnknown(WarnUseOfUnknown<JsonServiceInternalUnknown> _) { }

    public T To<T>(string json) => throw new NotImplementedException();
}