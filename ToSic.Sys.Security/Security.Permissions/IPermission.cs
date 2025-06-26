namespace ToSic.Sys.Security.Permissions;

public interface IPermission
{
    /// <summary>
    /// The condition in this permission - like "this rule applies to admins"
    /// The condition is usually a text-code by the hosting CMS
    /// </summary>
    string Condition { get; }

    /// <summary>
    /// The identity this rule should apply to 
    /// This is usually a user guid or group-id; exact specs vary based on the hosting CMS
    /// </summary>
    string Identity { get; }

    /// <summary>
    /// The rights granted by this permission, usually a series of characters like r=read, u=update, etc.
    /// </summary>
    string Grant { get; }

    /// <summary>
    /// Special token to identify the owner, like "dnn:42"
    /// </summary>
    string Owner { get; }
}