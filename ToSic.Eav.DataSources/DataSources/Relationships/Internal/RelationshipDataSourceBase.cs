using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSource;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources.Internal;

/// <summary>
/// Base class for Children and Parents - since they share a lot of code
/// </summary>
public abstract class RelationshipDataSourceBase : Eav.DataSource.DataSourceBase
{
    /// <summary>
    /// These should be fully implemented in inheriting class, as the docs change from inheritance to inheritance
    /// </summary>
    [Configuration]
    public abstract string FieldName { get; }

    /// <summary>
    /// These should be fully implemented in inheriting class, as the docs change from inheritance to inheritance
    /// </summary>
    [Configuration]
    public abstract string ContentTypeName { get; }

    /// <summary>
    /// Will filter duplicate hits from the result.
    /// </summary>
    [Configuration(Fallback = true)]
    public bool FilterDuplicates => Configuration.GetThis(true);

    /// <summary>
    /// Constructor
    /// </summary>
    protected RelationshipDataSourceBase(MyServices services, string logName): base(services, logName)
    {
        ProvideOut(GetRelated);
    }

    private IImmutableList<IEntity> GetRelated()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();

        // Make sure we have an In - otherwise error
        var source = TryGetIn();
        if (source is null) return l.ReturnAsError(Error.TryGetInFailed());

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

        return l.ReturnAsOk(relationships.ToImmutableList());
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