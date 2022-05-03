using System;

namespace ToSic.Eav.WebApi.Context
{
    [Flags] public enum Ctx
    {
        // ReSharper disable once UnusedMember.Global
        None = 0,
        AppBasic = 1,
        AppEdit = 2,
        AppAdvanced = 4,

        Language = 8,
        Page = 16,
        Site = 32,
        System = 64,
        All = AppBasic | AppEdit | AppAdvanced | Language | Page | Site | System ,
    }

}
