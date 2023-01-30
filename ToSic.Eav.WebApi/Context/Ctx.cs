using System;

namespace ToSic.Eav.WebApi.Context
{
    [Flags] public enum Ctx
    {
        // ReSharper disable once UnusedMember.Global
        None = 0,
        AppBasic = 1 << 0,
        AppEdit = 1 << 1,
        AppAdvanced = 1 << 2,
        Language = 1 << 3,
        Page = 1 << 4,
        Site = 1 << 5,
        System = 1 << 6,
        User = 1 << 7,
        Features = 1 << 8,
        All = AppBasic | AppEdit | AppAdvanced | Language | Page | Site | System | User | Features,
        General = AppBasic | AppEdit | AppAdvanced | Language | Page | Site | System | User | Features,
    }

}
