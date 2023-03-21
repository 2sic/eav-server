using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Generics;
using ToSic.Eav.LookUp;
using ToSic.Eav.Plumbing;
using static System.StringComparer;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Provide configuration to new data sources.
    /// </summary>
    public class DataSourceConfiguration: IConfiguration
    {
        public DataSourceConfiguration(IDictionary<string, string> values)
        {
            if (values == null) return;
            _values = values.ToInvariant();
        }
        private readonly IDictionary<string, string> _values;

        public DataSourceConfiguration(params string[] values)
        {
            var cleaned = values?.Where(v => v.HasValue()).ToList() ?? new List<string>();
            if (cleaned.SafeNone()) return;
            _values = cleaned
                .Select(v => v.Split('='))
                .Where(pair => pair.Length == 2)
                .ToDictionary(pair => pair[0], pair => pair[1], InvariantCultureIgnoreCase);
        }

        public ILookUpEngine GetLookupEngineWip() => null;

        public IDictionary<string, string> GetValuesWip() => _values;
    }
}
