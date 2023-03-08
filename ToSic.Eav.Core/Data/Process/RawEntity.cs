using System;
using System.Collections.Generic;
using ToSic.Eav.Generics;

namespace ToSic.Eav.Data.Process
{
    public class RawEntity: RawEntityBase
    {
        public RawEntity()
        {
        }

        public RawEntity(Dictionary<string, object> values)
        {
            _values = values;
        }

        public Dictionary<string, object> Values
        {
            // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
            get => _values ?? (_values = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase));
            set => _values = value?.ToInvariant() ?? _values;
        }
        private Dictionary<string, object> _values;


        public override Dictionary<string, object> GetProperties(CreateFromNewOptions options) => _values;
    }
}
