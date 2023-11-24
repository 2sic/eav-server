using System;

namespace ToSic.Eav.Apps.Environment;

public interface IEnvironmentLogger
{
    void LogException(Exception ex);
}