namespace ToSic.Sys.Security.Permissions;

public static class MultiPermissionCheckExtensions
{
    extension(IMultiPermissionCheck multiCheck)
    {
        /// <summary>
        /// Check if any of the permissions allow reading draft data (for initializing app readers)
        /// </summary>
        public PermissionCheckInfo EnsureAnyReadDraft()
            => multiCheck.EnsureAny(GrantSets.ReadDraft);

        /// <summary>
        /// Ensure that any! checks pass
        /// </summary>
        /// <returns>True if all pass, false if any one fails</returns>
        public PermissionCheckInfo EnsureAny(List<Grants> grants)
            => multiCheck.Log.Quick(()
                => multiCheck
                       .PermissionCheckers
                       .Select(set => set.Value.Ensure(grants))
                       .FirstOrDefault(result => result.Allowed)
                   ?? new PermissionCheckInfo(false, Conditions.Undefined, "No permission check passed")
            );

        ///// <summary>
        ///// Verify that *all* checks pass
        ///// </summary>
        ///// <returns>True if all pass, false if any one fails</returns>
        //public bool UserMayOnAll(List<Grants> grants)
        //    => multiCheck.EnsureAllWithError(grants, out _);

        /// <summary>
        /// Verify that *all* checks pass
        /// </summary>
        /// <param name="grants"></param>
        /// <returns>True if all pass, false if any one fails</returns>
        public PermissionCheckInfo EnsureAll(List<Grants> grants)
        {
            var l = multiCheck.Log.Fn<PermissionCheckInfo>($"Doing {nameof(EnsureAll)} for {multiCheck.GetType().Name}");

            var permCheckers = multiCheck.PermissionCheckers;
            l.A($"{nameof(multiCheck.PermissionCheckers)}: {permCheckers.Count}");

            var notAllowed = permCheckers
                .Select(set => set.Value.Ensure(grants))
                .FirstOrDefault(permInfo => !permInfo.Allowed);
            
            return notAllowed != null
                ? l.Return(notAllowed, "false")
                : l.Return(new(true, Conditions.Undefined, "All permission checks passed"));
        }
     
        ///// <summary>
        ///// Verify that *all* checks pass
        ///// </summary>
        ///// <param name="grants"></param>
        ///// <param name="error">Out error message to use to throw upstream</param>
        ///// <returns>True if all pass, false if any one fails</returns>
        //public bool EnsureAllWithError(List<Grants> grants, out string? error)
        //{
        //    var l = multiCheck.Log.Fn<bool>($"Doing {nameof(EnsureAllWithError)} for {multiCheck.GetType().Name}");

        //    var permCheckers = multiCheck.PermissionCheckers;
        //    l.A($"{nameof(multiCheck.PermissionCheckers)}: {permCheckers.Count}");

        //    foreach (var set in permCheckers)
        //        if (!set.Value.Ensure(grants, out error))
        //            return l.ReturnFalse();

        //    error = null;
        //    return l.ReturnTrue();
        //}
    }
}