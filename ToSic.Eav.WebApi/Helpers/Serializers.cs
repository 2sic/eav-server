using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Serializers;

namespace ToSic.Eav.WebApi.Helpers
{
    public static class Serializers
    {
        // I must keep the serializer so it can be configured from outside if necessary
        //private Serializer _serializer;
        public static Serializer GetSerializerWithGuidEnabled()
        {
            // if (_serializer != null) return _serializer;
            var serializer = Factory.Resolve<Serializer>();
            serializer.IncludeGuid = true;
            return serializer;
        }
    }
}
