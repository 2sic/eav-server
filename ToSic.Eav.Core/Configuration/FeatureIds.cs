using System;

namespace ToSic.Eav.Configuration
{
    public class FeatureIds
    {
        // Important: these names are public - don't ever change them
        public static readonly Guid PublicForms = new Guid("d93baf71-74c6-4956-9fe0-8281acdfd14a");
        public static readonly Guid PublicUpload = new Guid("79b9f5f8-d104-458b-8e8f-9f4a11c5935e");
        public static readonly Guid UseAdamInWebApi = new Guid("ecdab0f6-4692-4544-b1e7-72581f489f6a");

        public static readonly Guid PermissionCheckUserId = new Guid("47c71ee9-ac7b-45bf-a08b-dfc8ce7c7775");
        public static readonly Guid PermissionCheckGroups = new Guid("0fd479cc-300f-47fd-88fd-8f2fe092bc09");


        // Beta
        public static readonly Guid PasteImageClipboard = new Guid("f6b8d6da-4744-453b-9543-0de499aa2352");

        public static readonly Guid WysiwygPasteFormatted = new Guid("1b13e0e6-a346-4454-a1e6-2fb18c047d20");

        // new for 2sxc 10.0
        public static readonly Guid EditFormPreferAngularJs = new Guid("51da2093-f75a-4750-aea2-b45562fc4d51");

        // new for 2sxc 10.02
        public static readonly Guid BlockFileIdLookupIfNotInSameApp = new Guid("702f694c-53bd-4d03-b75c-4dad9c4fb852");

        // todo: this name isn't final yet
        public static readonly Guid WebApiOptionsAllowLocal = new Guid("99fe8253-eb9e-46e3-af7b-b994c19ecfd6");
    }
}
