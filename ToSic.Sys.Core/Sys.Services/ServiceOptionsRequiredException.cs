namespace ToSic.Sys.Services;

public class ServiceOptionsRequiredException()
    : NotSupportedException("This service requires options to be set before using. " +
                            "Default options are not supported. " +
                            "You are probably using the wrong Generator, " +
                            "or you are accessing MyOptions within the constructor.");