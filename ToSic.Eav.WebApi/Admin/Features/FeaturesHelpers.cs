//using System.Collections.Generic;
//using System.Linq;
//using ToSic.Eav.Configuration;

//namespace ToSic.Eav.WebApi.Admin.Features
//{
//    internal class FeaturesHelpers
//    {
//        /// <summary>
//        /// if the user has full edit permissions, he may also get the un-public features
//        /// otherwise just the public Ui features
//        /// </summary>
//        internal static IList<FeatureState> FeaturesUiBasedOnPermissions(IFeaturesInternal features,
//            bool userHasPublishRight, bool userIsAdmin)
//            => userIsAdmin
//                ? features.All.ToList()
//                : features.EnabledUi.Where(f => userHasPublishRight || f.Public).ToList();
//    }
//}
