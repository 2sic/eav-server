namespace ToSic.Eav.Data
{
    public interface IHasIdentityNameId
    {
        /// <summary>
        /// Primary identifier of an object which has this property.
        /// It will be unique and used as an ID where needed.
        /// </summary>
        string NameId { get; }
    }
}
