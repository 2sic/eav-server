using System;
using ToSic.Eav.Data;
using ToSic.Eav.Data.PropertyLookup;

namespace ToSic.Eav.Apps.Tests.PropertyLookupAndStack
{
    public class PropertyLookupTestBase
    {
        protected object GetResult(IPropertyLookup source, string fieldName, PropertyLookupPath path = null) =>
            GetRequest(source, fieldName, path).Result;

        protected PropertyRequest GetRequest(IPropertyLookup source, string fieldName, PropertyLookupPath path = null) =>
            source.FindPropertyInternal(fieldName, Array.Empty<string>(), null, path.KeepOrNew().Add("Start", fieldName));

        protected PropertyRequest GetRequestPath(IPropertyStack source, string fieldPath, PropertyLookupPath path = null) =>
            source.InternalGetPath(fieldPath, Array.Empty<string>(), null, path.KeepOrNew().Add("Start", fieldPath));

    }
}
