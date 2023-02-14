using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Base class for Children and Parents - since they share a lot of code
    /// </summary>

    public abstract class MetadataDataSourceBase : DataSource
    {
        /// <summary>
        /// These should be fully implemented in inheriting class, as the docs change from inheritance to inheritance
        /// </summary>
        [Configuration]
        public abstract string ContentTypeName { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        protected MetadataDataSourceBase(Dependencies dependencies, string logName): base(dependencies, logName)
        {
            Provide(GetMetadata);
        }

        private IImmutableList<IEntity> GetMetadata() => Log.Func(l =>
        {
            Configuration.Parse();

            // Make sure we have an In - otherwise error
            if (!GetRequiredInList(out var originals))
                return (originals, "error");

            var typeName = ContentTypeName;
            if (string.IsNullOrWhiteSpace(typeName)) typeName = null;
            l.A($"Content Type Name: {typeName}");

            IEnumerable<IEntity> relationships = SpecificGet(originals, typeName);

            return (relationships.ToImmutableList(), "ok");
        });

        protected abstract IEnumerable<IEntity> SpecificGet(IImmutableList<IEntity> originals, string typeName);
    }
}
