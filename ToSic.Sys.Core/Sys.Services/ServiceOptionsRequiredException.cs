namespace ToSic.Sys.Services;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ServiceOptionsRequiredException(Type parent)
    : NotSupportedException($"The service '{parent.FullName}' requires options to be set before using. " +
                            "Default options are not supported. " +
                            "You are probably using the wrong Generator, " +
                            "or you are accessing MyOptions within the constructor.");