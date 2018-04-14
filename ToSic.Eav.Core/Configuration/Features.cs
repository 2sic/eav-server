using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;

namespace ToSic.Eav.Configuration
{
    public class Features
    {
        public const string TypeName = "FeaturesConfiguration";
        public const string FeaturesField = "Features";
        public const string SignatureField = "Signature";

        private static ImmutableDictionary<Guid, Feature> _all; 
        private static ImmutableDictionary<Guid, Feature> _stored; 

        // todo: differentiate between stored and used...

        internal static ImmutableDictionary<Guid, Feature> Stored => _stored ?? (_stored = Load());

        internal static ImmutableDictionary<Guid, Feature> Dic => _all ?? (_all = Merge(_stored, Catalog));

        public static IEnumerable<Feature> All => Dic.Values;

        public static IEnumerable<Feature> Ui => Dic.Values
            .Where(f => f.Enabled)
            .Where(f => f.Ui == true
                        || Catalog.Features.Any(cf => cf.Id == f.Id && cf.Ui == true));

        public static bool Enabled(Guid id) => Dic.TryGetValue(id, out var feat) && feat.Enabled;

        private static ImmutableDictionary<Guid, Feature> Load()
        {
            try
            {
                var entity = Global.For(TypeName);
                var featStr = entity?.GetBestValue(FeaturesField)?.ToString();

                FeatureList feats = null;
                if (featStr?.StartsWith("{") ?? false)
                    feats = JsonConvert.DeserializeObject<FeatureList>(featStr);

                if (feats != null)
                    return feats.Features.ToDictionary(f => f.Id, f => f).ToImmutableDictionary();
            }
            catch 
            {
                /* ignore */
            }
            return new Dictionary<Guid, Feature>().ToImmutableDictionary();
        }

        private static ImmutableDictionary<Guid, Feature> Merge(ImmutableDictionary<Guid, Feature> config, FeatureList cat)
        {
            return config.Values.Select(f =>
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
            })
            .ToImmutableDictionary(f => f.Id, f => f);

        }

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
