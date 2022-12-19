using System;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// Note: these values are possibly used in other Apps - not often, but it's possible
    /// Just make sure we don't use them
    /// TODO:
    /// 1. Create a new standard to use this
    /// 2. Then mark as obsolete
    /// 3. And log to obsolete that this feature shouldn't be used any more
    /// </summary>
    [PrivateApi("this should probably never be public, as we want to rename things at will")]
    public class FeatureIds
    {
        // Important: these names are public - don't ever change them
        public static Guid PublicForms => BuiltInFeatures.PublicEditForm.Guid;
        public static Guid PublicUpload => BuiltInFeatures.PublicUploadFiles.Guid;
        public static Guid UseAdamInWebApi => BuiltInFeatures.SaveInAdamApi.Guid;
        public static Guid PermissionCheckUserId => BuiltInFeatures.PermissionCheckUsers.Guid;
        public static Guid PermissionCheckGroups => BuiltInFeatures.PermissionCheckGroups.Guid;
    }
}
