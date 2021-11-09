using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Base class for Children and Parents - since they share a lot of code
    /// </summary>

    public abstract class RelationshipDataSourceBase : DataSourceBase
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
        protected RelationshipDataSourceBase()
        {
            Provide(GetRelated);
            ConfigMask(nameof(FieldName));
            ConfigMask(nameof(ContentTypeName));
            ConfigMask(nameof(FilterDuplicates) + "||true");
        }

        private IImmutableList<IEntity> GetRelated()
        {
            var wrapLog = Log.Call<IImmutableList<IEntity>>();

            Configuration.Parse();

            // Make sure we have an In - otherwise error
            if (!GetRequiredInList(out var originals))
                return wrapLog("error", originals);

            var fieldName = FieldName;
            if (string.IsNullOrWhiteSpace(fieldName)) fieldName = null;
            Log.Add($"Field Name: {fieldName}");
            
            var typeName = ContentTypeName;
            if (string.IsNullOrWhiteSpace(typeName)) typeName = null;
            Log.Add($"Content Type Name: {typeName}");

            var find = InnerGet(fieldName, typeName);

            var relationships = originals
                .SelectMany(o => find(o));

            if (FilterDuplicates)
                relationships = relationships.Distinct();

            return wrapLog(null, relationships.ToImmutableList());
        }

        /// <summary>
        /// Construct function for the get of the related items
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        protected abstract Func<IEntity, IEnumerable<IEntity>> InnerGet(string fieldName, string typeName);

    }
}
