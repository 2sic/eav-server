using System.Collections.Generic;
using static ToSic.Eav.Security.Grants;

namespace ToSic.Eav.Security.Permissions;

public static class GrantSets
{
    /// <summary>
    /// If any of the following permissions are given, the user
    /// may read somethig - but it's not clear yet what may be read
    /// </summary>
    /// <remarks>
    /// It's important to note that create-grants don't provide
    /// read-grants
    /// </remarks>
    public static List<Grants> ReadSomething =
    [
        Approve,
        Read,
        Grants.ReadDraft,
        Update,
        UpdateDraft,
        Develop,
        Full
    ];

    public static List<Grants> ReadDraft =
    [
        Approve,
        DeleteDraft,
        Grants.ReadDraft,
        UpdateDraft,
        Delete,
        Develop,
        Full,
        Update
    ];

    public static List<Grants> ReadPublished =
    [
        Approve,
        Read,
        Delete,
        Develop,
        Full,
        Update
    ];

    public static List<Grants> WriteSomething =
    [
        Delete,
        DeleteDraft,
        Create,
        CreateDraft,
        Develop,
        Full,
        Approve,
        UpdateDraft,
        Update
    ];

    // ReSharper disable once UnusedMember.Global
    public static List<Grants> WriteDraft =
    [
        Delete,
        DeleteDraft,
        CreateDraft,
        Develop,
        Full,
        Approve,
        UpdateDraft,
        Update
    ];

    public static List<Grants> WritePublished =
    [
        Create,
        Update,
        Approve,
        Full,
        Develop
    ];


    public static List<Grants> UpdateSomething =
    [
        Develop,
        Full,
        Approve,
        UpdateDraft,
        Update
    ];


    public static List<Grants> DeleteSomething =
    [
        Delete,
        DeleteDraft
    ];


    public static List<Grants> CreateSomething =
    [
        Create,
        CreateDraft,
        Full,
        Develop
    ];
}