﻿namespace ToSic.Lib.Core.Tests.LoggingTests;

/// <summary>
/// Note: you won't see any code, because it inherits all the tests from the base class.
/// It just has a different way of getting the Log - from a HasLog.
/// </summary>

public class HasLogAdd: LogAdd
{
    protected class ThingWithLog() : ServiceBase("");

    /// <summary>
    /// Create a log for the test. Can be overriden in inheriting classes. 
    /// </summary>
    protected override (ILog LogForAdd, Log RealLog) LogFactory(string name = "")
    {
        var log = (Log)new ThingWithLog().Log;
        return (log, log);
    }
}