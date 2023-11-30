using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav.Apps.State
{
    public interface IAppStateChanges
    {
        event EventHandler AppStateChanged;
    }
}
