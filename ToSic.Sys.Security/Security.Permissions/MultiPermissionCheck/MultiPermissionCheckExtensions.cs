namespace ToSic.Sys.Security.Permissions;

public static class MultiPermissionCheckExtensions
{
    extension(IMultiPermissionCheck multiCheck)
    {
        /// <summary>
        /// Ensure that any! checks pass
        /// </summary>
        /// <returns>True if all pass, false if any one fails</returns>
        public bool EnsureAny(List<Grants> grants)
            => multiCheck.Log.Quick(() => multiCheck.PermissionCheckers.Any(set => set.Value.Ensure(grants, out _)));

        /// <summary>
        /// Verify that *all* checks pass
        /// </summary>
        /// <returns>True if all pass, false if any one fails</returns>
        public bool UserMayOnAll(List<Grants> grants)
            => multiCheck.EnsureAllWithError(grants, out _);

        /// <summary>
        /// Verify that *all* checks pass
        /// </summary>
        /// <param name="grants"></param>
        /// <param name="error">Out error message to use to throw upstream</param>
        /// <returns>True if all pass, false if any one fails</returns>
        public bool EnsureAllWithError(List<Grants> grants, out string? error)
        {
            var l = multiCheck.Log.Fn<bool>($"Doing {nameof(EnsureAllWithError)} for {multiCheck.GetType().Name}");

            var permCheckers = multiCheck.PermissionCheckers;
            l.A($"{nameof(multiCheck.PermissionCheckers)}: {permCheckers.Count}");

            foreach (var set in permCheckers)
                if (!set.Value.Ensure(grants, out error))
                    return l.ReturnFalse();

            error = null;
            return l.ReturnTrue();
        }
    }
}