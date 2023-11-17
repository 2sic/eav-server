﻿using System;
using ToSic.Eav.Context;
using ToSic.Eav.Run;

namespace ToSic.Testing.Shared.Platforms
{
    public class TestPlatformPatronPerfectionist: IPlatformInfo
    {
        public virtual string Name => "Test";

        public Version Version => new Version(27, 42, 00);

        public string Identity => "564b5b5c-a18e-45a0-b810-b77fd7e8484c";
    }
}
