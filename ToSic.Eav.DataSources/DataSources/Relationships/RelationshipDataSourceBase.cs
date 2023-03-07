using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Base class for Children and Parents - since they share a lot of code
    /// </summary>

    public abstract class RelationshipDataSourceBase : DataSource
    {
        /// <summary>
        /// These should be fully implemented in inheriting class, as the docs change from inheritance to inheritance
        /// </summary>
        [Configuration]
        public abstract string FieldName { get; set; }

        /// <summary>
        /// These should be fully implemented in inheriting class, as the docs change from inheritance to inheritance
        /// </summary>
        [Configuration]
        public abstract string ContentTypeName { get; set; }

        /// <summary>
        /// Will filter duplicate hits from the result.
        /// </summary>
        [Configuration(Fallback = true)]
        public bool FilterDuplicates
        {
            get => Configuration.GetThis(true);
            set => Configuration.SetThis(value);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected RelationshipDataSourceBase(MyServices services, string logName): base(services, logName)
        {
            Provide(GetRelated);
        }

        private IImmutableList<IEntity> GetRelated() => Log.Func(() =>
        {
            Configuration.Parse();

            // Make sure we have an In - otherwise error
            var source = TryGetIn();
            if (source is null) return (Error.TryGetInFailed(this), "error");

            var fieldName = FieldName;
            if (string.IsNullOrWhiteSpace(fieldName)) fieldName = null;
            Log.A($"Field Name: {fieldName}");

            var typeName = ContentTypeName;
            if (string.IsNullOrWhiteSpace(typeName)) typeName = null;
            Log.A($"Content Type Name: {typeName}");

            var find = InnerGet(fieldName, typeName);

            var relationships = source
                .SelectMany(o => find(o));

            if (FilterDuplicates)
                relationships = relationships.Distinct();

            return (relationships.ToImmutableList(), "ok");
        });

        /// <summary>
        /// Construct function for the get of the related items
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        [PrivateApi]
        protected abstract Func<IEntity, IEnumerable<IEntity>> InnerGet(string fieldName, string typeName);

    }
}
