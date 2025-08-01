﻿namespace ToSic.Eav.Apps.Sys.State;

partial class AppState
{
    // ReSharper disable once ChangeFieldTypeToSystemThreadingLock
    private readonly object _transactionLock = new();

    /// <summary>
    /// Experiment to try to ensure the same entity cannot be created twice
    /// </summary>
    /// <param name="parentLog"></param>
    /// <param name="transaction"></param>
    public void DoInLock(ILog parentLog, Action transaction)
    {
        var l = parentLog.Fn();
        try
        {
            lock (_transactionLock)
            {
                var lInner = parentLog.Fn("in lock");
                transaction();
                lInner.Done();
            }
        }
        finally
        {
            l.Done();
        }
    }
}