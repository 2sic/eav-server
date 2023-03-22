using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Base class for Children and Parents - since they share a lot of code
    /// </summary>

    public abstract class MetadataDataSourceBase : DataSource
    {
        /// <remarks>
        /// These should be fully implemented in inheriting class, as the docs change from inheritance to inheritance
        /// </remarks>
        public abstract string ContentTypeName { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        protected MetadataDataSourceBase(MyServices services, string logName): base(services, logName)
        {
            ProvideOut(GetMetadata);
        }

        private IImmutableList<IEntity> GetMetadata() => Log.Func(l =>
        {
            Configuration.Parse();

            // Make sure we have an In - otherwise error
            var source = TryGetIn();
            if (source is null) return (Error.TryGetInFailed(), "error");

            var typeName = ContentTypeName;
            if (string.IsNullOrWhiteSpace(typeName)) typeName = null;
            l.A($"Content Type Name: {typeName}");

            var relationships = SpecificGet(source, typeName);

            return (relationships.ToImmutableList(), "ok");
        });

        protected abstract IEnumerable<IEntity> SpecificGet(IImmutableList<IEntity> originals, string typeName);
    }
}
