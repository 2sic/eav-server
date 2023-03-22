namespace ToSic.Eav.Context
{
    public class ContextResolverUserPermissions: IContextResolverUserPermissions
    {
        private readonly IContextOfUserPermissions _userPermissions;

        public ContextResolverUserPermissions(IContextOfUserPermissions userPermissions)
        {
            _userPermissions = userPermissions;
        }

        public IContextOfUserPermissions UserPermissions() => _userPermissions;
    }
}
