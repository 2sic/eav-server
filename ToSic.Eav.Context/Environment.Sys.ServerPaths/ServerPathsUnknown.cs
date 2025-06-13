﻿#pragma warning disable CS9113 // Parameter is unread.
namespace ToSic.Eav.Environment.Sys.ServerPaths;

public sealed class ServerPathsUnknown(WarnUseOfUnknown<ServerPathsUnknown> _): ServerPathsBase, IIsUnknown
{
    public override string FullAppPath(string virtualPath) => "unknown-path, please implement IServerPaths";

    public override string FullContentPath(string virtualPath) => "unknown-path, please implement IServerPaths";

    protected override string FullPathOfReference(int id) => $"unknown-path for id {id}, please implement IServerPaths";
}