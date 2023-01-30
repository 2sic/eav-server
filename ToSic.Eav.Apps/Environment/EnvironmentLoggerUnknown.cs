﻿using System;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;

namespace ToSic.Eav.Apps.Environment
{
    public sealed class EnvironmentLoggerUnknown: IEnvironmentLogger, IIsUnknown
    {
        public EnvironmentLoggerUnknown(WarnUseOfUnknown<EnvironmentLoggerUnknown> _) { }

        public void LogException(Exception ex)
        {
            // do nothing
        }
    }
}
