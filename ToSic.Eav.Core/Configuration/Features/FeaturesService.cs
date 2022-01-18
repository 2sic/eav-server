using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Configuration
{
    [PrivateApi("hide implementation")]
    public class FeaturesService: IFeaturesInternal
    {
        public IEnumerable<Feature> All => (_all ?? (_all = Merge(Stored, FeaturesCatalog.Initial)));
        private static List<Feature> _all;

        public IEnumerable<Feature> Ui => All.Where(f => f.Enabled && f.Ui);

        public bool Enabled(Guid guid) => All.Any(f => f.Guid == guid && f.Enabled);
        
        public bool Enabled(IEnumerable<Guid> guids) => guids.All(Enabled);

        public bool Enabled(params string[] nameIds) => nameIds.All(name => All.Any(f => f.NameId == name && f.Enabled));

        public bool Valid => ValidInternal;
        public static bool ValidInternal;
        
        public bool Enabled(IEnumerable<Guid> features, string message, out FeaturesDisabledException exception)
        {
            // ReSharper disable PossibleMultipleEnumeration
            var enabled = Enabled(features);
            exception = enabled ? null : new FeaturesDisabledException(message + " - " + MsgMissingSome(features), features);
            // ReSharper restore PossibleMultipleEnumeration
            return enabled;
        }

        [PrivateApi]
        public string MsgMissingSome(IEnumerable<Guid> ids)
            => $"Features {string.Join(", ", ids.Where(i => !Enabled(i)).Select(id => $"{InfoLinkRoot}{id}"))} not enabled - see also {HelpLink}";

        #region Links

        /// <inheritdoc />
        public string HelpLink
        {
            get => _helpLink;
            set => _helpLink = value;
        }
        private static string _helpLink = "https://2sxc.org/help?tag=features";

        /// <inheritdoc />
        public string InfoLinkRoot
        {
            get => _infoLinkRoot;
            set => _infoLinkRoot = value;
        }
        private static string _infoLinkRoot = "https://2sxc.org/r/f/";

        #endregion

        #region Static Caches

        [PrivateApi]
        public FeatureListStored Stored
        {
            get => _stored;
            set
            {
                _stored = value;
                _all = null;
            }
        }
        private static FeatureListStored _stored;


        private static List<Feature> Merge(FeatureListStored config, IReadOnlyCollection<FeatureDefinition> cat)
        {
            var feats = config.Features.Select(f =>
            {
                var inCat = cat.FirstOrDefault(c => c.Guid == f.Id)
                    ?? new FeatureDefinition(f.Id);
                return new Feature(inCat)
                {
                    Enabled = f.Enabled,
                    Expires = f.Expires,
                };
            }).ToList();

            return feats;
        }


        /// <summary>
        /// Just for debugging
        /// </summary>
        [PrivateApi]
        public long CacheTimestamp { get; set; }

        #endregion
    }
}
