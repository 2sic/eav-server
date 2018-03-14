namespace ToSic.Eav.Apps.Interfaces
{
    public interface IPermissions
    {
        // todo: probably merge with the other permissionscontroller
        bool UserMayEditContent(IInstanceInfo instanceInfo);



    }
}
