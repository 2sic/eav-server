﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Base class for Children and Parents - since they share a lot of code
    /// </summary>

    public abstract class MetadataDataSourceBase : DataSourceBase
    {
        /// <summary>
        /// These should be fully implemented in inheriting class, as the docs change from inheritance to inheritance
        /// </summary>
        public abstract string ContentTypeName { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        protected MetadataDataSourceBase()
        {
            Provide(GetMetadata);
            ConfigMask(nameof(ContentTypeName));
        }

        private IImmutableList<IEntity> GetMetadata()
        {
            var wrapLog = Log.Call<IImmutableList<IEntity>>();

            Configuration.Parse();

            // Make sure we have an In - otherwise error
            if (!GetRequiredInList(out var originals))
                return wrapLog("error", originals);

            var typeName = ContentTypeName;
            if (string.IsNullOrWhiteSpace(typeName)) typeName = null;
            Log.Add($"Content Type Name: {typeName}");

            IEnumerable<IEntity> relationships = SpecificGet(originals, typeName);

            return wrapLog(null, relationships.ToImmutableList());
        }

        protected abstract IEnumerable<IEntity> SpecificGet(IImmutableList<IEntity> originals, string typeName);
        //{
        //    var find = InnerGet(typeName);

        //    var relationships = originals
        //        .SelectMany(o => find(o));

        //    relationships = Postprocess(relationships);
        //    return relationships;
        //}

        ///// <summary>
        ///// Construct function for the get of the related items
        ///// </summary>
        ///// <param name="fieldName"></param>
        ///// <param name="typeName"></param>
        ///// <returns></returns>
        //[PrivateApi]
        //protected abstract Func<IEntity, IEnumerable<IEntity>> InnerGet(string typeName);

        //protected virtual IEnumerable<IEntity> Postprocess(IEnumerable<IEntity> results) => results;
    }
}