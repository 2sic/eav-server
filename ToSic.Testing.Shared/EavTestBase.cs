﻿using ToSic.Eav;

namespace ToSic.Testing.Shared
{
    public abstract class EavTestBase
    {
        public static T Resolve<T>() => Factory.Resolve<T>();
    }
}
