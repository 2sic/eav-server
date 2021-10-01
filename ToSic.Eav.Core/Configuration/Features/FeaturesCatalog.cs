using System.Collections.Generic;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi]
    internal class FeaturesCatalog
    {
        /// <summary>
        /// The catalog contains known features, and knows if they are used in the UI
        /// This is important, because the installation specific list often won't know about
        /// Ui or not. 
        /// </summary>
        /// <remarks>
        /// this is a temporary solution, because most features are from 2sxc (not eav)
        /// so later on this must be injected or something
        /// </remarks>
        [PrivateApi]
        public static FeatureList Initial = new FeatureList(new List<Feature>
        {
            // released features
            new Feature(FeatureIds.PublicForms, true, false),
            new Feature(FeatureIds.PublicUpload, true, false),
            new Feature(FeatureIds.UseAdamInWebApi, false, false),

            new Feature(FeatureIds.PermissionCheckUserId, true, false),
            new Feature(FeatureIds.PermissionCheckGroups, true, false),

            // Beta features
            new Feature(FeatureIds.PasteImageClipboard, true, true),
            //new Feature(FeatureIds.Angular5Ui, false,false),
            new Feature(FeatureIds.WysiwygPasteFormatted, true, true),

            // 2sxc 9.43+
            new Feature(FeatureIds.EditFormPreferAngularJs, true, true),
            new Feature(FeatureIds.WebApiOptionsAllowLocal, true, false),

            // 2sxc 10.24+
            new Feature(FeatureIds.WebFarm, false, false),
        });
    }
}
