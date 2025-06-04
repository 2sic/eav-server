using ToSic.Eav.Data.PropertyStack.Sys;
using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Apps.Tests.PropertyLookupAndStack;

public class PropertyLookupTestBase
{

    protected object GetResult(IPropertyLookup source, string fieldName, PropertyLookupPath path = null) =>
        GetRequest(source, fieldName, path).Result;

    protected PropReqResult GetRequest(IPropertyLookup source, string fieldName, PropertyLookupPath path = null) =>
        source.FindPropertyInternal(new(fieldName), path.KeepOrNew().Add("Start", fieldName));

    protected PropReqResult GetRequestPath(IPropertyStack source, string fieldPath, PropertyLookupPath path = null) =>
        source.InternalGetPath(new(fieldPath), path.KeepOrNew().Add("Start", fieldPath));

}