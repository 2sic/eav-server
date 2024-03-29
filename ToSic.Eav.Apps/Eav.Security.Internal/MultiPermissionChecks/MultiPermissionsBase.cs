﻿namespace ToSic.Eav.Security.Internal;

/// <summary>
/// A permission checker which is initialized with various items which must be checked
/// Calling Ensure or similar will verify that all permission checks succeed
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public abstract class MultiPermissionsBase<TServices>(TServices services, string logName)
    : ServiceBase<TServices>(services, logName), IMultiPermissionCheck
    where TServices : MyServicesBase
{
    /// <summary>
    /// All the permission checks that will be used
    /// </summary>
    public Dictionary<string, IPermissionCheck> PermissionCheckers
        => _permissionCheckers ??= InitializePermissionChecks();
    private Dictionary<string, IPermissionCheck> _permissionCheckers;

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
    public bool EnsureAll(List<Grants> grants, out string error)
    {
        var wrap = Log.Fn<bool>();
        foreach (var set in PermissionCheckers)
            if (!set.Value.Ensure(grants, out error))
                return wrap.ReturnFalse();

        error = null;
        return wrap.ReturnTrue();
    }

    /// <summary>
    /// Ensure that any! checks pass
    /// </summary>
    /// <param name="grants"></param>
    /// <returns>True if all pass, false if any one fails</returns>
    public bool EnsureAny(List<Grants> grants) => Log.Func(() =>
        PermissionCheckers.Any(set => set.Value.Ensure(grants, out _)));

}