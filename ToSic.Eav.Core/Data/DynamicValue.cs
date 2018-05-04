using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;

namespace ToSic.Eav.Data
{
    public class DynamicValue : DynamicObject
    {
        private readonly string _underlyingValue;

        public DynamicValue(string initializer)
        {
            _underlyingValue = initializer;
        }

        public DynamicValue(JToken jToken)
        {
            part = jToken;
        }

        private bool _initialized;

        JObject root;
        private JToken part;

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // todo: continue here, this block isn't tested; may be better to outsource this into an own type?
            if (part != null)
            {
                // todo: check if it exists, try/find
                var found = part[binder.Name];
                result = found;
                return result != null;
            }

            if(!_initialized) FirstAccess();

            // if deserialization wasn't possible, return the string
            if (root == null)
            {
                result = _underlyingValue;
                return true;
            }

            // try to get the value from the json object
            if(!root.TryGetValue(binder.Name, out var jResult))
            {
                result = null;
                return false;
            }

            // if it has children, return the "navigator"
            result = jResult.HasValues 
                ? jResult 
                : (jResult as JValue)?.Value;

            return true;
        }

        private void FirstAccess()
        {
            if (_initialized) return;
            _initialized = true; // make sure it will never run twice
            try
            {
                root = JObject.Parse(_underlyingValue);
            }
            catch
            {
                /* ignore */
            }

        }
    }
}
