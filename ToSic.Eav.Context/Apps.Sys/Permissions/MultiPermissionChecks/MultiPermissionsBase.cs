//using ToSic.Sys.Security.Permissions;

//namespace ToSic.Eav.Apps.Sys.Permissions;

///// <summary>
///// A permission checker which is initialized with various items which must be checked
///// Calling Ensure or similar will verify that all permission checks succeed
///// </summary>
//[ShowApiWhenReleased(ShowApiMode.Never)]
//public abstract class MultiPermissionsBase<TServices, TOptions>(TServices services, string logName, object[]? connect = default)
//    : ServiceWithSetup<TServices, TOptions>(services, logName, connect: connect), IMultiPermissionCheck
//    where TServices : IDependencies
//    where TOptions : class, new()
//{
//    /// <inheritdoc/>
//    [field: AllowNull, MaybeNull]
//    public abstract Dictionary<string, IPermissionCheck> PermissionCheckers { get; }


//    /// <inheritdoc/>
//    public bool UserMayOnAll(List<Grants> grants) => EnsureAll(grants, out _);

//    /// <inheritdoc/>>
//    public bool EnsureAll(List<Grants> grants, out string? error) => this.EnsureAllInternal(grants, out error);

//}
