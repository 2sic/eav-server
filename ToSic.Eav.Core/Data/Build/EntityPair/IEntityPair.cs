namespace ToSic.Eav.Data.Build;

/// <summary>
/// Interface marking <see cref="EntityPair{TPartner}"/>s.
/// This is for internal use, to ensure covariance.
/// </summary>
/// <typeparam name="TPartner"></typeparam>
/// <remarks>
/// Added in 15.04
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IEntityPair<out TPartner>: ICanBeEntity
{
    TPartner Partner { get; }
}