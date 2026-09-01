using System.Runtime.CompilerServices;
using ToSic.Eav.Models.Factory;

namespace ToSic.Eav.Models;

// ReSharper disable UnusedMember.Global

/// <summary>
/// Extension methods to convert <see cref="IEntity"/> or lists of IEntity to Models.
/// WIP v21
/// TODO DOCS
/// </summary>
/// <remarks>
/// </remarks>
[WorkInProgressApi("WIP v21")]
public static partial class ToModelExtensions
{
    private static IModelFactory AssertFactory(IModelFactory? factory, [CallerMemberName] string? methodName = default)
        => factory ?? throw new ArgumentNullException(nameof(factory), $"You need to provide a model factory to convert to models in method {methodName}");
}