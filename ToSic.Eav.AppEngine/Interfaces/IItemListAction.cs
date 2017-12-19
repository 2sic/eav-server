using System;
using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Apps.Interfaces
{
    /// <summary>
    /// This describes an action-command which manipulates a list
    /// </summary>
    public interface IItemListAction
    {
        //bool Change(List<int?> ids);

        //bool Change(List<Guid?> ids);

        List<IEntity> Change(List<IEntity> ids);
    }
}
