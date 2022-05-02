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
        public static Guid PublicForms => BuiltInFeatures.PublicEditForm.Guid;
        public static Guid PublicUpload => BuiltInFeatures.PublicUploadFiles.Guid;
        public static Guid UseAdamInWebApi => BuiltInFeatures.SaveInAdamApi.Guid;
        public static Guid PermissionCheckUserId => BuiltInFeatures.PermissionCheckUsers.Guid;
        public static Guid PermissionCheckGroups => BuiltInFeatures.PermissionCheckGroups.Guid;

        // Beta - never public, commented out 2022-01-03
        //public static Guid PasteImageClipboard => Features.PasteImageFromClipboard.Id; // new Guid("f6b8d6da-4744-453b-9543-0de499aa2352");
        //public static Guid WysiwygPasteFormatted => Features.WysiwygPasteFormatted.Id; // new Guid("1b13e0e6-a346-4454-a1e6-2fb18c047d20");

        // new for 2sxc 10.02 - never public, commented out 2022-01-03
        //public static Guid BlockFileIdLookupIfNotInSameApp => FeaturesDb.BlockFileResolveOutsideOfEntityAdam.Id; // new Guid("702f694c-53bd-4d03-b75c-4dad9c4fb852");
        //public static Guid WebFarm => FeaturesDb.WebFarmCache.Id; // new Guid("11c0fedf-16a7-4596-900c-59e860b47965");
    }
}
