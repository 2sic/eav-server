using System;
using ToSic.Eav.Documentation;

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
        public static Guid PublicForms => FeaturesBuiltIn.PublicEditForm.Guid; // new Guid("d93baf71-74c6-4956-9fe0-8281acdfd14a");
        public static Guid PublicUpload => FeaturesBuiltIn.PublicUploadFiles.Guid; // new Guid("79b9f5f8-d104-458b-8e8f-9f4a11c5935e");
        public static Guid UseAdamInWebApi => FeaturesBuiltIn.SaveInAdamApi.Guid; // new Guid("ecdab0f6-4692-4544-b1e7-72581f489f6a");
        public static Guid PermissionCheckUserId => FeaturesBuiltIn.PermissionCheckUsers.Guid; // new Guid("47c71ee9-ac7b-45bf-a08b-dfc8ce7c7775");
        public static Guid PermissionCheckGroups => FeaturesBuiltIn.PermissionCheckGroups.Guid; // new Guid("0fd479cc-300f-47fd-88fd-8f2fe092bc09");

        // Beta - never public, commented out 2022-01-03
        //public static Guid PasteImageClipboard => Features.PasteImageFromClipboard.Id; // new Guid("f6b8d6da-4744-453b-9543-0de499aa2352");
        //public static Guid WysiwygPasteFormatted => Features.WysiwygPasteFormatted.Id; // new Guid("1b13e0e6-a346-4454-a1e6-2fb18c047d20");

        // new for 2sxc 10.02 - never public, commented out 2022-01-03
        //public static Guid BlockFileIdLookupIfNotInSameApp => FeaturesDb.BlockFileResolveOutsideOfEntityAdam.Id; // new Guid("702f694c-53bd-4d03-b75c-4dad9c4fb852");
        //public static Guid WebFarm => FeaturesDb.WebFarmCache.Id; // new Guid("11c0fedf-16a7-4596-900c-59e860b47965");
    }
}
