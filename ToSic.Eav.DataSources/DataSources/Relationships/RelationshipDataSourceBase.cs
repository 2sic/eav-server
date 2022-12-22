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
        public abstract string FieldName { get; set; }

        /// <summary>
        /// These should be fully implemented in inheriting class, as the docs change from inheritance to inheritance
        /// </summary>
        public abstract string ContentTypeName { get; set; }


        public bool FilterDuplicates
        {
            get => Configuration.GetBool(nameof(FilterDuplicates));
            set => Configuration.SetBool(nameof(FilterDuplicates), value);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        protected RelationshipDataSourceBase(Dependencies dependencies, string logName): base(dependencies, logName)
        {
            Provide(GetRelated);
            ConfigMask(nameof(FieldName));
            ConfigMask(nameof(ContentTypeName));
            ConfigMask(nameof(FilterDuplicates) + "||true");
        }

        private IImmutableList<IEntity> GetRelated()
        {
            var wrapLog = Log.Fn<IImmutableList<IEntity>>();

            Configuration.Parse();

            // Make sure we have an In - otherwise error
            if (!GetRequiredInList(out var originals))
                return wrapLog.Return(originals, "error");

            var fieldName = FieldName;
            if (string.IsNullOrWhiteSpace(fieldName)) fieldName = null;
            Log.A($"Field Name: {fieldName}");
            
            var typeName = ContentTypeName;
            if (string.IsNullOrWhiteSpace(typeName)) typeName = null;
            Log.A($"Content Type Name: {typeName}");

            var find = InnerGet(fieldName, typeName);

            var relationships = originals
                .SelectMany(o => find(o));

            if (FilterDuplicates)
                relationships = relationships.Distinct();

            return wrapLog.Return(relationships.ToImmutableList());
        }

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
