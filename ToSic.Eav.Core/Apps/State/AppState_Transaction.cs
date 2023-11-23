using System;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps
{
    public partial class AppState
    {
        private readonly object _transactionLock = new();

        /// <summary>
        /// Experiment to try to ensure the same entity cannot be created twice
        /// </summary>
        /// <param name="parentLog"></param>
        /// <param name="transaction"></param>
        public void DoInLock(ILog parentLog, Action transaction) => parentLog.Do(() =>
        {
            try
            {
                lock (_transactionLock) 
                    parentLog.Do(transaction, message: "in lock");
            }
            finally
            {

            }
        });
    }
}
