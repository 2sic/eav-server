namespace ToSic.Sys.Exceptions;

public class MissingParameterException(string parameterName)
    : ArgumentException($"Missing parameter: {parameterName ?? "Reason not specified"}")
{
}
