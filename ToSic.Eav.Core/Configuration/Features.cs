using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Configuration
{
    public class Features
    {
        public const string TypeName = "FeaturesConfiguration";
        public const string FeaturesField = "Features";
        public const string SignatureField = "Signature";


        // todo: differentiate between stored and used...

        internal static FeatureList Stored => _stored ?? (_stored = Load());
        private static FeatureList _stored; 

        public static IEnumerable<Feature> All => (_merged ?? (_merged = Merge(Stored, Catalog))).Features;
        private static FeatureList _merged;

        public static IEnumerable<Feature> Ui => All
            .Where(f => f.Enabled && f.Ui == true);

        public static bool Enabled(Guid id) => All.Any(f => f.Id == id && f.Enabled);

        public static bool Enabled(IEnumerable<Guid> ids) => ids.All(Enabled);

        public static string InfoLink 
            => _infoLink ?? (_infoLink = Factory.Resolve<ISystemConfiguration>().FeaturesHelpLink);
        private static string _infoLink;

        public static string MsgMissingSome(Guid id) 
            => $"Feature {id} not enabled - see {InfoLink}";
        public static string MsgMissingSome(IEnumerable<Guid> ids) 
            => $"Features {string.Join(", ", ids.Where(i => !Enabled(i)).Select(i => i.ToString()))} not enabled - see {InfoLink}";

        /// <summary>
        /// Reset the features to force reloading of the features
        /// </summary>
        public static void Reset()
        {
            _merged = null;
            _stored = null;
        }

        private static FeatureList Load()
        {
            try
            {
                var entity = Global.For(TypeName);
                var featStr = entity?.GetBestValue(FeaturesField)?.ToString();

                FeatureList feats = null;
                if (featStr?.StartsWith("{") ?? false)
                    feats = JsonConvert.DeserializeObject<FeatureList>(featStr);

                if (feats != null)
                    return feats;
            }
            catch 
            {
                /* ignore */
            }
            CacheTimestamp = DateTime.Now.Ticks;
            return new FeatureList();
        }

        private static FeatureList Merge(FeatureList config, FeatureList cat)
        {
            var feats = config.Features.Select(f =>
            {
                var inCat = cat.Features.FirstOrDefault(c => c.Id == f.Id);
                return new Feature
                {
                    Id = f.Id,
                    Enabled = f.Enabled,
                    Expires = f.Expires,
                    Public = f.Public ?? inCat?.Public,
                    Ui = f.Ui ?? inCat?.Ui
                };
            }).ToList();

            return new FeatureList(feats);

        }

        public static long CacheTimestamp { get; private set; }

        public static bool CacheChanged(long compareTo) => compareTo != CacheTimestamp;




        /// <summary>
        /// The catalog contains known features, and knows if they are used in the UI
        /// This is important, because the installation specific list often won't know about
        /// Ui or not. 
        /// </summary>
        /// <remarks>
        /// this is a temporary solution, because most features are from 2sxc (not eav)
        /// so later on this must be injected or something
        /// </remarks>
        public static FeatureList Catalog = new FeatureList(new List<Feature>
        {
            // released features
            new Feature(FeatureIds.PublicForms, true, false, "public forms"),
            new Feature(FeatureIds.PublicUpload, true, false, "public allow file upload"),

            // Beta features
            new Feature(FeatureIds.PasteImageClipboard, true, true, "paste image from clipboard"),


        });


    }
}

