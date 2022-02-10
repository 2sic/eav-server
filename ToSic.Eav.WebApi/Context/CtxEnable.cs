using System;

namespace ToSic.Eav.WebApi.Context
{
    [Flags] public enum CtxEnable
    {
        // ReSharper disable once UnusedMember.Global
        None = 0,
        AppPermissions = 1 << 0,
        CodeEditor = 1 << 1,
        Query = 1 << 2,
        FormulaSave = 1 << 3,
        OverrideEditRestrictions = 1 << 4,
        All = AppPermissions | CodeEditor | Query| FormulaSave | OverrideEditRestrictions,
        EditUi = FormulaSave | OverrideEditRestrictions,
    }
}
