namespace ToSic.Sys.Utils.Types;

public class MissingConstructorException(string message) : MissingMethodException(message);