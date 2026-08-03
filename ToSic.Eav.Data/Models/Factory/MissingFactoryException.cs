namespace ToSic.Eav.Models.Factory;

internal class MissingFactoryException(string message) : InvalidOperationException(message);

internal class MissingSetupException(string message) : InvalidOperationException(message);

