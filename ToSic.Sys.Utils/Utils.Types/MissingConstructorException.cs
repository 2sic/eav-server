namespace ToSic.Sys.Utils.Types;

[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class MissingConstructorException(string message) : MissingMethodException(message);