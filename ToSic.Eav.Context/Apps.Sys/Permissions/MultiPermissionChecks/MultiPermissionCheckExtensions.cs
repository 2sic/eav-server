using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Apps.Sys.Permissions;

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

        public bool EnsureAllInternal(List<Grants> grants, out string? error)
        {
            var l = multiCheck.Log.Fn<bool>($"Doing {nameof(IMultiPermissionCheck.EnsureAll)} for {multiCheck.GetType().Name}");

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