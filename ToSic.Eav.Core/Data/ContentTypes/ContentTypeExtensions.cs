using ToSic.Eav.Data.Shared;

namespace ToSic.Eav.Data
{
    public static class ContentTypeExtensions
    {
        public static bool HasAncestor(this IContentType contentType)
        {
            var anc = contentType.GetDecorator<IAncestor>();
            return anc != null && anc.Id != 0;
        }

        public static bool HasPresetAncestor(this IContentType contentType)
        {
            var anc = contentType.GetDecorator<IAncestor>();
            return anc != null && anc.Id == Constants.PresetContentTypeFakeParent;
        }
    }
}
