﻿using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources;

/// <summary>
/// Base class for Children and Parents - since they share a lot of code
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class MetadataDataSourceBase : Eav.DataSource.DataSourceBase
{
    /// <remarks>
    /// These should be fully implemented in inheriting class, as the docs change from inheritance to inheritance
    /// </remarks>
    public abstract string ContentTypeName { get; }


    /// <summary>
    /// Constructor
    /// </summary>
    protected MetadataDataSourceBase(MyServices services, string logName): base(services, logName)
    {
        ProvideOut(GetMetadata);
    }

    private IImmutableList<IEntity> GetMetadata()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();

        // Make sure we have an In - otherwise error
        var source = TryGetIn();
        if (source is null) return l.ReturnAsError(Error.TryGetInFailed());

        var typeName = ContentTypeName;
        if (string.IsNullOrWhiteSpace(typeName)) typeName = null;
        l.A($"Content Type Name: {typeName}");

        var relationships = SpecificGet(source, typeName);

        return l.ReturnAsOk(relationships.ToImmutableList());
    }

    protected abstract IEnumerable<IEntity> SpecificGet(IImmutableList<IEntity> originals, string typeName);
}