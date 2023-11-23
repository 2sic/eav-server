using System;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Data.PropertyLookup
{
    public class PropReqSpecs: ICanDump
    {
        public PropReqSpecs(string field): this(field, null) { }

        public PropReqSpecs(string field, string[] dimensions, ILog logOrNull = null, bool treatEmptyAsDefault = true)
        {
            Field = field;
            Dimensions = dimensions ?? Array.Empty<string>();
            TreatEmptyAsDefault = treatEmptyAsDefault;            LogOrNull = logOrNull;
        }

        public readonly string Field;

        public readonly string[] Dimensions;

        public readonly bool TreatEmptyAsDefault;

        /// <summary>
        /// Log is null if no logging should happen, or a real logger if it's in use
        /// </summary>
        public readonly ILog LogOrNull;

        public PropReqSpecs ForOtherField(string field) => new(field, Dimensions, LogOrNull, TreatEmptyAsDefault);

        public PropReqSpecs SubLog(string title) =>
            new(Field, Dimensions, LogOrNull.SubLogOrNull(title), TreatEmptyAsDefault);
        public PropReqSpecs SubLog(string title, bool enabled) => new(Field, Dimensions,
            LogOrNull.SubLogOrNull(title, enabled), TreatEmptyAsDefault);

        public string Dump() => _dump ??= $"{nameof(PropReqSpecs)} {{ {nameof(Field)}:{Field}, {nameof(Dimensions)}:{string.Join(",", Dimensions)} }}";
        private string _dump;
    }
}
