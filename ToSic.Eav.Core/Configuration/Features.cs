using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

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

        public static bool IsEnabled(Guid id) => All.Any(f => f.Id == id && f.Enabled);

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
            Timestamp = DateTime.Now;
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

        public static DateTime Timestamp { get; private set; }

        public static bool CompareCache(DateTime compareTo) => compareTo.CompareTo(Timestamp) != 0;

        /// <summary>
        /// The catalog contains known features, and knows if they are used in the UI
        /// This is important, because the installation specific list often won't know about
        /// Ui or not. 
        /// </summary>
        public static FeatureList Catalog = new FeatureList(new List<Feature>
        {
            // paste clipboard
            new Feature(new Guid("f6b8d6da-4744-453b-9543-0de499aa2352"), true, true)
        });


    }
}

