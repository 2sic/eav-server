namespace ToSic.Eav.Models.Sys;

internal enum ModelCreationStatus
{
    Success,
    ErrorCreateInstance,
    InvalidCast,
    RequiresFactory,
    MissingSetup,
    UnknownError,
}