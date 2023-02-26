using System.Collections.Generic;

namespace ToSic.Eav.Data
{
    public partial class ContentType: IHasDecorators<IContentType>
    {
        public List<IDecorator<IContentType>> Decorators { get; }
        //    =>
        //    _decorators ?? (_decorators = new List<IDecorator<IContentType>>());
        //private List<IDecorator<IContentType>> _decorators;
    }
}
