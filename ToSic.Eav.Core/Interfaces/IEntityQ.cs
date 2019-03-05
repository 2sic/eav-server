using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
    public interface IEntityQ: IEntity
    {
        List<IEntityQ> Children(string field = null, string type = null);
        List<IEntityQ> Parents(string type = null, string field = null);

        object Get(string field);

        T Get<T>(string field);
    }
}
