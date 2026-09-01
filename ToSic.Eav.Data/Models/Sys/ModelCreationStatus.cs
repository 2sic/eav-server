namespace ToSic.Eav.Models.Sys;

internal enum ModelCreationStatus
{
    Success,
    GetTargetTypeFails,
    CreateInstanceFails,
    InvalidCast,
    RequiresFactory,
    MissingSetup,
    UnknownError,
}