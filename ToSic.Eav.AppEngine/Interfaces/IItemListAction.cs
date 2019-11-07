using System.Collections.Generic;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// This describes an action-command which manipulates a list
    /// </summary>
    public interface IItemListAction
    {
        List<IEntity> Change(List<IEntity> ids);
    }
}
