namespace ToSic.Eav.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Added in 13.02
    /// IMPORTANT: Don't cache this object, as some info inside it can change during runtime
    /// </remarks>
    public class ContentTypeDetails: EntityBasedType
    {
        public const string ContentTypeTypeName = "ContentType";

        public ContentTypeDetails(IEntity entity) : base(entity) { }

        public string Icon => GetThis<string>(null);

        public string DynamicChildrenField => GetThis<string>(null);

        public string Description => GetThis<string>(null);

        public string AdditionalSettings => GetThis<string>(null);
    }
}
