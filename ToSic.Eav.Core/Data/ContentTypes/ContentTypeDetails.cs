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

        public string Icon => Get<string>("Icon", null);

        public string DynamicChildrenField => Get<string>("DynamicChildrenField", null);

        public string Description => Get<string>("Description", null);
    }
}
