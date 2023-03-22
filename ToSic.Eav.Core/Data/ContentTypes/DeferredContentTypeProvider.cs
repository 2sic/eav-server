using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Data.ContentTypes
{
    /// <summary>
    /// Special helper class for delayed construction of ContentTypes so they can be immutable
    /// </summary>
    public class DeferredContentTypeProvider : IDeferredContentTypeProvider
    {
        public List<IContentType> Source { get; } = new List<IContentType>();
        public DeferredContentTypeProvider()
        {
        }

        public IContentType LazyTypeGenerator(int appId, string name, string nameId, IContentType fallback)
        {
            var delayedType = new ContentTypeWrapper(() => Source.FirstOrDefault(t => t.Is(nameId)) ?? fallback);
            return delayedType;
        }
    }
}
