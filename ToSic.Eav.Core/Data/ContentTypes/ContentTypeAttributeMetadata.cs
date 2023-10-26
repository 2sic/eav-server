using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Metadata;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// WIP
    /// #SharedFieldDefinition
    /// </summary>
    [PrivateApi]
    public class ContentTypeAttributeMetadata: MetadataOf<int>
    {
        public Guid? SourceGuid { get; }

        // todo: move title generation to here using name/type?
        public ContentTypeAttributeMetadata(int key, string name, ValueTypes type, Guid? sourceGuid = default, IReadOnlyCollection<IEntity> items = default, IHasMetadataSource appSource = default, Func<IHasMetadataSource> deferredSource = default)
            : base(targetType: (int)TargetTypes.Attribute, key: key, title: $"{name} ({type})", items: items, appSource: appSource, deferredSource: deferredSource)
        {
            SourceGuid = sourceGuid;
        }

        /// <summary>
        /// Replace the key if we have a guid pointing to another Attribute
        /// </summary>
        protected override int Key => _key.Get(() =>
        {
            return SourceGuid == null 
                ? base.Key 
                : SourceAttribute?.AttributeId ?? base.Key;
            //// Try to check source if it has the referred guid
            //if (Source.MainSource is AppState app)
            //{
            //    var attributes = app.ContentTypes.SelectMany(ct => ct.Attributes);
            //    var SourceAttribute = attributes.FirstOrDefault(a => a.Guid == SourceGuid.Value);
            //    if (SourceAttribute != null) return SourceAttribute.AttributeId;
            //}
            //// Use base key as default
            //return base.Key;
        });

        private readonly GetOnce<int> _key = new GetOnce<int>();

        private IContentTypeAttribute SourceAttribute => _sourceAttribute.Get(() =>
        {
            if (SourceGuid == null) return null;
            if (!(Source.MainSource is AppState app)) return null;
            var attributes = app.ContentTypes.SelectMany(ct => ct.Attributes);
            return attributes.FirstOrDefault(a => a.Guid == SourceGuid.Value);
        });
        private readonly GetOnce<IContentTypeAttribute> _sourceAttribute = new GetOnce<IContentTypeAttribute>();

        protected override List<IEntity> LoadFromProviderInsideLock(IList<IEntity> additions = default)
        {
            // Most common case - just behave as if this didn't do anything special
            // Note that we'll ignore additions, as these are only used in ContentTypeMetadata and not here
            var cached = base.LoadFromProviderInsideLock();
            if (cached.Any() || SourceAttribute == null) return cached;

            // AllWithHidden is still empty, but we have a SourceAttribute
            // Common reason is that the SourceAttribute has directly attached metadata, so we'll try that
            if (!(SourceAttribute.Metadata is ContentTypeAttributeMetadata sourceMd)) return cached;
            return sourceMd.Source.SourceDirect?.List == null 
                ? cached 
                : base.LoadFromProviderInsideLock(sourceMd.Source.SourceDirect.List?.ToList());
        }
    }
}
