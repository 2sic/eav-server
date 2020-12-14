using System;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    public partial class AppState
    {
        private readonly object _transactionLock = new object();

        /// <summary>
        /// Experiment to try to ensure the same entity cannot be created twice
        /// </summary>
        /// <param name="parentLog"></param>
        /// <param name="transaction"></param>
        public void DoInLock(ILog parentLog, Action transaction)
        {
            var wrapLog = parentLog.Call();

            try
            {
                lock (_transactionLock)
                {
                    var inLock = parentLog.Call(message: "in lock");
                    transaction();
                    inLock.Invoke(null);
                }
            }
            finally{

            }

            wrapLog.Invoke("ok");
        }
    }
}
