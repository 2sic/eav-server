﻿namespace ToSic.Eav.Internal.Configuration;

public interface IDbConfiguration
{
    /// <summary>
    /// Db Connection String used in the Eav-Connector
    /// </summary>
    string ConnectionString { get; set; }
}