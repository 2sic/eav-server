//using ToSic.Eav.Data;

namespace ToSic.Lib.DI
{
    public interface ISwitchableService /*: IHasIdentityNameId*/
    {
        bool IsViable();

        int Priority { get; }

        /// <summary>
        /// Primary identifier of an object which has this property.
        /// It will be unique and used as an ID where needed.
        /// </summary>
        string NameId { get; }
    }
}
