using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Metadata;
using ToSic.Lib.Documentation;

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
        public ContentTypeAttributeMetadata(int key, string name, ValueTypes type, Guid? sourceGuid = default, IReadOnlyCollection<IEntity> items = default, /*IHasMetadataSource appSource = default,*/ Func<IHasMetadataSource> deferredSource = default)
            : base(targetType: (int)TargetTypes.Attribute, key: key, title: $"{name} ({type})", items: items, /*appSource: appSource,*/ deferredSource: deferredSource)
        {
            SourceGuid = sourceGuid;
        }

        /// <summary>
        /// Replace the key if we have a guid pointing to another Attribute
        /// </summary>
        protected override int Key
        {
            get
            {
                if (_key != null) return _key.Value;
                if (SourceGuid == null) return (_key = base.Key).Value;

                // Use base key as default
                _key = base.Key;

                // Try to check source if it has the referred guid
                if (Source.MainSource is AppState app)
                {
                    var attributes = app.ContentTypes.SelectMany(ct => ct.Attributes);
                    var ofGuid = attributes.FirstOrDefault(a => a.Guid == SourceGuid.Value);
                    if (ofGuid != null) _key = ofGuid.AttributeId;
                }
                
                return _key.Value;
            }
        }

        private int? _key;
    }
}
