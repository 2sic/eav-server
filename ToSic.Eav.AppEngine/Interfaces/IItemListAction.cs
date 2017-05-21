using System.Collections.Generic;

namespace ToSic.Eav.Apps.Interfaces
{
    /// <summary>
    /// This describes an action-command which manipulates a list
    /// </summary>
    public interface IItemListAction
    {
        bool Change(List<int?> ids);
    }
}
