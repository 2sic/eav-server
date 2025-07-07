﻿using ToSic.Sys.Users.Permissions;

namespace ToSic.Eav.DataSources.Sys;

/// <summary>
/// Base class for Children and Parents - since they share a lot of code
/// </summary>
/// <remarks>
/// * in v18.00 we optimized it to also check draft-permissions of the returned data - previously this was not checked
/// </remarks>
public abstract class RelationshipDataSourceBase : DataSourceBase
{
    /// <summary>
    /// These should be fully implemented in inheriting class, as the docs change from inheritance to inheritance
    /// </summary>
    [Configuration]
    public abstract string? FieldName { get; }

    /// <summary>
    /// These should be fully implemented in inheriting class, as the docs change from inheritance to inheritance
    /// </summary>
    [Configuration]
    public abstract string? ContentTypeName { get; }

    /// <summary>
    /// Will filter duplicate hits from the result.
    /// </summary>
    [Configuration(Fallback = true)]
    public bool FilterDuplicates => Configuration.GetThis(true);

    /// <summary>
    /// Constructor
    /// </summary>
    protected RelationshipDataSourceBase(Dependencies services, ICurrentContextUserPermissionsService userPermissions, string logName): base(services, logName, connect: [userPermissions])
    {
        _userPermissions = userPermissions;
        ProvideOut(GetRelated);
    }

    private readonly ICurrentContextUserPermissionsService _userPermissions;

    private IImmutableList<IEntity> GetRelated()
    {
        var l = Log.Fn<IImmutableList<IEntity>>();
        Configuration.Parse();

        // Make sure we have an In - otherwise error
        var source = TryGetIn();
        if (source is null)
            return l.ReturnAsError(Error.TryGetInFailed());

        var fieldName = FieldName;
        if (string.IsNullOrWhiteSpace(fieldName)) fieldName = null;
        l.A($"Field Name: {fieldName}");

        var typeName = ContentTypeName;
        if (string.IsNullOrWhiteSpace(typeName)) typeName = null;
        l.A($"Content Type Name: {typeName}");

        var find = InnerGet(fieldName, typeName);

        var relationships = source
            .SelectMany(o => find(o))
            .ToList();

        // In case the current user should not see draft data,
        // we must ensure that we didn't accidentally include any.
        // Because it could be that the original data was public, but a related item was not.
        if (!(_userPermissions.UserPermissions()?.ShowDraftData ?? false))
            relationships = relationships.Where(e => e.IsPublished).ToList();

        // ReSharper disable PossibleMultipleEnumeration
        l.A($"{nameof(FilterDuplicates)}: {FilterDuplicates}");
        var result = FilterDuplicates
            ? relationships.Distinct()
            : relationships;

#if DEBUG
        var relsList = relationships.ToList();
        var distinctList = result.ToList();
#endif

        return l.ReturnAsOk(result.ToImmutableOpt());
        // ReSharper restore PossibleMultipleEnumeration
    }

    /// <summary>
    /// Construct function for the get of the related items
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="typeName"></param>
    /// <returns></returns>
    [PrivateApi]
    protected abstract Func<IEntity, IEnumerable<IEntity>> InnerGet(string? fieldName, string? typeName);

}