namespace ToSic.Eav.Apps.Interfaces
{
    public interface IPermissions
    {
        // Todo: convert to method receiving the InstanceInfo
        // todo: probably merge with the other permissionscontroller
        bool UserMayEditContent { get; }

        
    }
}
