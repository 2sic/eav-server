﻿using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Apps.Sys.Permissions;

/// <summary>
/// A permission checker which is initialized with various items which must be checked
/// Calling Ensure or similar will verify that all permission checks succeed
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class MultiPermissionsBase<TServices>(TServices services, string logName, object[]? connect = default)
    : ServiceBase<TServices>(services, logName, connect: connect), IMultiPermissionCheck
    where TServices : MyServicesBase
{
    /// <summary>
    /// All the permission checks that will be used
    /// </summary>
    [field: AllowNull, MaybeNull]
    public Dictionary<string, IPermissionCheck> PermissionCheckers
        => field ??= InitializePermissionChecks();

    #region abstract methods

    protected abstract Dictionary<string, IPermissionCheck> InitializePermissionChecks();

    #endregion


    /// <summary>
    /// Verify that all checks pass
    /// </summary>
    /// <param name="grants"></param>
    /// <returns>True if all pass, false if any one fails</returns>
    public bool UserMayOnAll(List<Grants> grants) => EnsureAll(grants, out _);

    /// <summary>
    /// Ensure that all! checks pass
    /// </summary>
    /// <param name="grants"></param>
    /// <param name="error">Out error message to use to throw upstream</param>
    /// <returns>True if all pass, false if any one fails</returns>
    public bool EnsureAll(List<Grants> grants, out string? error)
    {
        var l = Log.Fn<bool>($"Doing {nameof(EnsureAll)} for {GetType().Name}");

        var permCheckers = PermissionCheckers;
        l.A($"{nameof(PermissionCheckers)}: {permCheckers.Count}");

        foreach (var set in permCheckers)
            if (!set.Value.Ensure(grants, out error))
                return l.ReturnFalse();

        error = null;
        return l.ReturnTrue();
    }

    /// <summary>
    /// Ensure that any! checks pass
    /// </summary>
    /// <param name="grants"></param>
    /// <returns>True if all pass, false if any one fails</returns>
    public bool EnsureAny(List<Grants> grants) => Log.Quick(() =>
        PermissionCheckers.Any(set => set.Value.Ensure(grants, out _)));

}