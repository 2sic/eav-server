using System;
using ToSic.Lib.Logging;

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
            var wrapLog = parentLog.Fn();

            try
            {
                lock (_transactionLock)
                {
                    var inLock = parentLog.Fn(message: "in lock");
                    transaction();
                    inLock.Done();
                }
            }
            finally{

            }

            wrapLog.Done("ok");
        }
    }
}
